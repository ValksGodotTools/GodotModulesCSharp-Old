using Newtonsoft.Json;
using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Valk.Modules.Netcode
{
    public class WebClient : Node
    {
        public static HttpClient Client { get; set; }

        public WebClient() => Client = new HttpClient();

        public static async Task<string> Post(string url, Dictionary<string, string> values)
        {
            try
            {
                var data = new FormUrlEncodedContent(values);
                var response = await Client.PostAsync($"http://{url}", data);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (HttpRequestException)
            {
                return $"Failed POST request, could not reach http://{url}";
            }
        }

        public static async Task<T> Get<T>(string url)
        {
            try
            {
                var response = await Client.GetAsync($"http://{url}");
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<T>(content);
                return obj;
            }
            catch (HttpRequestException)
            {
                return default(T);
            }
        }

        public static string GetExternalIp()
        {
            string externalIpString = new System.Net.WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            return IPAddress.Parse(externalIpString).ToString();
        }
    }
}
