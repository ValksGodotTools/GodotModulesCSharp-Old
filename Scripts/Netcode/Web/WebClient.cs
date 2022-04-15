using Timer = System.Timers.Timer; // ambitious reference between Godot.Timer and System.Timers.Timer

using Newtonsoft.Json;
using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Timers;
using Valk.Modules.Netcode.Server;

namespace Valk.Modules.Netcode
{
    public class WebClient : Node
    {
        public static HttpClient Client { get; set; }
        private static int FailedPingAttempts { get; set; }
        public const int WEB_PING_INTERVAL = 10000;

        public WebClient() 
        {
            Client = new HttpClient();
            Client.Timeout = TimeSpan.FromSeconds(5);
        }

        public static async Task<WebServerResponse<string>> Post(string url, Dictionary<string, string> values)
        {
            try
            {
                var data = new FormUrlEncodedContent(values);
                var response = await Client.PostAsync($"http://{url}", data);
                var content = await response.Content.ReadAsStringAsync();
                return new WebServerResponse<string>{
                    Status = WebServerStatus.OK,
                    Content = content
                };
            }
            catch (Exception e)
            {
                return new WebServerResponse<string>{
                    Status = WebServerStatus.ERROR,
                    Exception = e
                };
            }
        }

        public static async Task<WebServerResponse<T>> Get<T>(string url)
        {
            try
            {
                var response = await Client.GetAsync($"http://{url}");
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<T>(content);
                return new WebServerResponse<T>{
                    Status = WebServerStatus.OK,
                    Content = obj
                };
            }
            catch (Exception e)
            {
                return new WebServerResponse<T>{
                    Status = WebServerStatus.ERROR,
                    Exception = e
                };
            }
        }

        public static async void OnTimerPingMasterServerEvent(System.Object source, ElapsedEventArgs e) 
        {
            var res = await WebClient.Post("localhost:4000/api/ping", new Dictionary<string, string> {{ "Name", UIGameServers.CurrentLobby.Name }});

            if (res.Status == WebServerStatus.ERROR) 
            {
                FailedPingAttempts++;
                if (FailedPingAttempts > 3)
                    ENetServer.TimerPingMasterServer.Stop();
            }
        }

        public static string GetExternalIp()
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
