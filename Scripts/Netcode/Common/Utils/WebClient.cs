using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Timers;
using Timer = System.Timers.Timer; // ambitious reference between Godot.Timer and System.Timers.Timer

namespace GodotModules.Netcode
{
    public class WebClient : IDisposable
    {
        public HttpClient Client { get; set; }
        public bool ConnectionAlive { get; set; }
        public Task<WebServerResponse<LobbyListing[]>> TaskGetServers => Get<LobbyListing[]>("servers/get");
        public string ExternalIp { get; set; }
        private int FailedPingAttempts { get; set; }
        public string WEB_SERVER_IP = "localhost:4000";
        private const int WEB_PING_INTERVAL = 10000;
        private bool LogExceptions = false;
        public Timer TimerPingMasterServer { get; set; }

        public WebClient()
        {
            Client = new();
            // weird things will happen if you play around with these values
            // e.g. WebClient will no longer see web responses from master server
            // please do not touch these settings!!
            // ---------------------------------------------------------
            Client.Timeout = TimeSpan.FromMinutes(20);
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            Client.DefaultRequestHeaders.Add("Keep-Alive", "false");
            // ---------------------------------------------------------

            TimerPingMasterServer = new(WebClient.WEB_PING_INTERVAL);
            TimerPingMasterServer.AutoReset = true;
            TimerPingMasterServer.Elapsed += new(OnTimerPingMasterServerEvent);

            try
            {
                ExternalIp = new System.Net.WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            }
            catch (Exception e)
            {
                ExternalIp = "";
                System.Console.WriteLine($"Failed to get external IP {e.Message}\n{e.StackTrace}");
            }
        }

        public async Task<WebServerResponse<string>> PostError(string errorText, string errorDescription)
            => await PostAsync("errors/post", new Dictionary<string, string> {
                    { "error", errorText },
                    { "description", errorDescription }
                });

        public async Task<WebServerResponse<string>> RemoveLobbyAsync() => await PostAsync("server/remove", DataExternalIp);

        public async Task<WebServerResponse<string>> RemoveLobbyPlayerAsync() => await PostAsync("servers/players/remove", DataExternalIp);

        public async Task<WebServerResponse<string>> AddLobbyPlayerAsync() => await PostAsync("servers/players/add", DataExternalIp);

        private Dictionary<string, string> DataExternalIp => new() { { "Ip", ExternalIp } };

        public async Task<WebServerResponse<string>> AddLobbyAsync(LobbyListing info)
        {
            var values = new Dictionary<string, string>
            {
                { "Name", info.Name },
                { "Ip", ExternalIp },
                { "Port", "" + info.Port },
                { "Description", info.Description },
                { "MaxPlayerCount", "" + info.MaxPlayerCount },
                { "LobbyHost", info.LobbyHost }
            };

            return await PostAsync("servers/add", values);
        }

        private async Task<WebServerResponse<string>> PostAsync(string path, Dictionary<string, string> values = null)
        {
            if (!ConnectionAlive)
                return new() { Status = WebServerStatus.OFFLINE };

            try
            {
                var data = new FormUrlEncodedContent(values);
                var response = await Client.PostAsync($"http://{WEB_SERVER_IP}/api/{path}", data);
                var content = await response.Content.ReadAsStringAsync();
                return new WebServerResponse<string>
                {
                    Status = WebServerStatus.OK,
                    Content = content
                };
            }
            catch (Exception e)
            {
                if (LogExceptions)
                {
                    var message = $"Failed to POST to http://{WEB_SERVER_IP}/api/{path} {e.Message}";
                    Logger.LogWarning(message);
                }
                return new WebServerResponse<string>
                {
                    Status = WebServerStatus.ERROR,
                    Exception = e
                };
            }
        }

        public async Task UpdateIsAlive()
        {
            try
            {
                var response = await Client.GetAsync($"http://{WEB_SERVER_IP}/api/ping");
                await response.Content.ReadAsStringAsync();
                ConnectionAlive = true;
            }
            catch (Exception)
            {
                ConnectionAlive = false;
            }
        }

        public async Task<WebServerResponse<T>> Get<T>(string path)
        {
            if (!ConnectionAlive)
                return new() { Status = WebServerStatus.OFFLINE };

            try
            {
                var response = await Client.GetAsync($"http://{WEB_SERVER_IP}/api/{path}");
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<T>(content);
                return new WebServerResponse<T>
                {
                    Status = WebServerStatus.OK,
                    Content = obj
                };
            }
            catch (Exception e)
            {
                if (LogExceptions)
                {
                    var message = $"Failed to GET from http://{WEB_SERVER_IP}/api/{path} {e.Message}";
                    Logger.LogWarning(message); // no need to notify user of this kind of error
                }
                return new WebServerResponse<T>
                {
                    Status = WebServerStatus.ERROR,
                    Exception = e
                };
            }
        }

        public async void OnTimerPingMasterServerEvent(System.Object source, ElapsedEventArgs args)
        {
            var res = await PostAsync("servers/ping", new Dictionary<string, string> { { "Name", NetworkManager.CurrentLobby.Name } });
            if (res.Status == WebServerStatus.ERROR)
            {
                FailedPingAttempts++;
                if (FailedPingAttempts >= 3)
                    TimerPingMasterServer.Stop();

                return;
            }

            FailedPingAttempts = 0; // reset failed ping attempts if a ping gets through
        }

        public void Dispose()
        {
            TimerPingMasterServer.Dispose();
            Client.Dispose();
        }
    }

    public struct WebServerResponse<T>
    {
        public WebServerStatus Status { get; set; }
        public Exception Exception { get; set; }
        public T Content { get; set; }
    }

    public enum WebServerStatus
    {
        OK,
        ERROR,
        OFFLINE
    }
}