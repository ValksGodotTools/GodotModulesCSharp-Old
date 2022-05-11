using Newtonsoft.Json;
using System.Net.Http;

namespace GodotModules.Netcode
{
    public class WebClient
    {
        private HttpClient _client;

        public WebClient()
        {
            _client = new();
            _client.Timeout = TimeSpan.FromMinutes(20);
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            _client.DefaultRequestHeaders.Add("Keep-Alive", "false");
        }
    }
}