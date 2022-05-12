using Newtonsoft.Json;
using System.Net.Http;

namespace GodotModules.Netcode
{
    public class WebClient : IDisposable
    {
        public bool ConnectionAlive { get; set; }
        public bool LogExceptions { get; set; }
        public string ExternalIp { get; set; }

        private readonly HttpClient _client;
        private readonly string _ip;

        public WebClient(string ip)
        {
            _ip = ip;
            _client = new();
            _client.Timeout = TimeSpan.FromMinutes(20);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            _client.DefaultRequestHeaders.Add("Keep-Alive", "false");
        }

        public async Task CheckConnectionAsync()
        {
            try
            {
                var response = await _client.GetAsync($"http://{_ip}/api/ping");
                await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(ExternalIp))
                    ExternalIp = GetExternalIp();

                ConnectionAlive = true;
            }
            catch (Exception)
            {
                ConnectionAlive = false;
            }
        }

        public async Task<WebServerResponse<string>> PostErrorAsync(string errorText, string errorDescription)
            => await PostAsync("errors/post", new Dictionary<string, string> {
                    { "error", errorText },
                    { "description", errorDescription }
                });
        public async Task<WebServerResponse<string>> RemoveLobbyAsync() => await PostAsync("server/remove", DataExternalIp);
        public async Task<WebServerResponse<string>> RemoveLobbyPlayerAsync() => await PostAsync("servers/players/remove", DataExternalIp);
        public async Task<WebServerResponse<string>> AddLobbyPlayerAsync() => await PostAsync("servers/players/add", DataExternalIp);

        /*public async Task<WebServerResponse<string>> AddLobbyAsync(LobbyListing info)
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
        }*/

        private Dictionary<string, string> DataExternalIp => new() { { "Ip", ExternalIp } };
        
        private string GetExternalIp()
        {
            try
            {
                return new System.Net.WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            }
            catch (Exception e)
            {
                GM.LogWarning($"Failed to get external IP {e.Message}\n{e.StackTrace}");
                return "";
            }
        }

        public async Task<WebServerResponse<T>> Get<T>(string path)
        {
            if (!ConnectionAlive)
                return new() { Status = WebServerStatus.OFFLINE };

            var url = $"http://{_ip}/api/{path}";

            try
            {
                var response = await _client.GetAsync(url);
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
                    var message = $"Failed to GET from {url} {e.Message}";
                    GM.LogWarning(message); // no need to notify user of this kind of error
                }
                return new WebServerResponse<T>
                {
                    Status = WebServerStatus.ERROR,
                    Exception = e
                };
            }
        }

        private async Task<WebServerResponse<string>> PostAsync(string path, Dictionary<string, string> values = null)
        {
            if (!ConnectionAlive)
                return new() { Status = WebServerStatus.OFFLINE };

            var url = $"http://{_ip}/api/{path}";

            try
            {
                var data = new FormUrlEncodedContent(values);
                var response = await _client.PostAsync(url, data);
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
                    var message = $"Failed to POST to {url} {e.Message}";
                    GM.LogWarning(message);
                }
                return new WebServerResponse<string>
                {
                    Status = WebServerStatus.ERROR,
                    Exception = e
                };
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }

    public class WebServerResponse<T>
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