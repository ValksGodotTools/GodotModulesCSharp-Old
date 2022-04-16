using Godot;
using GodotModules.Netcode.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

namespace GodotModules.Netcode
{
    public class WebClient : Node
    {
        public HttpClient Client { get; set; }
        public Task<WebServerResponse<LobbyListing[]>> TaskGetServers { get; set; }
        private static int FailedPingAttempts { get; set; }
        private const string WEB_SERVER_IP = "localhost:4000";
        public const int WEB_PING_INTERVAL = 10000;
        private bool LogExceptions = false;

        public WebClient()
        {
            Client = new HttpClient();
            Client.Timeout = TimeSpan.FromSeconds(5);
        }

        public async Task<WebServerResponse<string>> Post(string path, Dictionary<string, string> values)
        {
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
                    GD.Print($"Failed to POST to http://{WEB_SERVER_IP}/api/{path} {e.Message}");
                return new WebServerResponse<string>
                {
                    Status = WebServerStatus.ERROR,
                    Exception = e
                };
            }
        }

        public async Task<WebServerResponse<T>> Get<T>(string path)
        {
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
                    GD.Print($"Failed to GET from http://{WEB_SERVER_IP}/api/{path} {e.Message}");
                return new WebServerResponse<T>
                {
                    Status = WebServerStatus.ERROR,
                    Exception = e
                };
            }
        }

        public async static void OnTimerPingMasterServerEvent(System.Object source, ElapsedEventArgs e)
        {
            var res = await GameManager.WebClient.Post("ping", new Dictionary<string, string> { { "Name", UIGameServers.Instance.CurrentLobby.Name } });

            if (res.Status == WebServerStatus.ERROR)
            {
                FailedPingAttempts++;
                if (FailedPingAttempts >= 3)
                    ENetServer.Instance.TimerPingMasterServer.Stop();

                return;
            }

            FailedPingAttempts = 0; // reset failed ping attempts if a ping gets through
        }

        /*private static int FailedPostAttempts { get; set; }
        private static System.Timers.Timer TimerPostAttempts { get; set; }
        public async static void OnTimerPostMasterServerEvent(System.Object source, ElapsedEventArgs e)
        {

        }*/

        public string GetExternalIp()
        {
            string externalIpString = new System.Net.WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            return IPAddress.Parse(externalIpString).ToString();
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
        ERROR
    }
}