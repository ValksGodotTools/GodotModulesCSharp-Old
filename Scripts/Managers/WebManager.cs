using Godot;

namespace GodotModules
{
    public class WebManager
    {
        public string Ip { get; set; }
        public bool ConnectionAlive { get; private set; }

        private string _externalIp;

        private readonly WebRequests _webRequests;

        public WebManager(WebRequests webRequests, string ip)
        {
            _webRequests = webRequests;
            Ip = ip.IsAddress() ? ip : "localhost:4000";
        }

        public async Task CheckConnectionAsync(CancellationTokenSource cts)
        {
            var res = await _webRequests.GetAsync($"http://{Ip}/api/ping", cts);

            if (res == null)
                return;

            ConnectionAlive = res.Error == Error.Ok;
        }

        public async Task<WebServerResponse> RemoveLobbyPlayerAsync(CancellationTokenSource cts) => await PostAsync("servers/players/remove", DataExternalIp, cts);
        public async Task<WebServerResponse> AddLobbyPlayerAsync(CancellationTokenSource cts) => await PostAsync("servers/players/add", DataExternalIp, cts);
        public async Task<WebServerResponse> RemoveLobbyAsync(CancellationTokenSource cts) => await PostAsync("server/remove", DataExternalIp, cts);
        /*public async Task<WebServerResponse> AddLobbyAsync(CancellationTokenSource cts) =>
            await PostAsync("servers/add", lobby)*/

        private Dictionary<string, string> DataExternalIp => new() { { "Ip", _externalIp } };

        public async Task<WebServerResponse> SendErrorAsync(string errorText, string errorDescription, CancellationTokenSource cts) => 
            await PostAsync("errors/post", new Dictionary<string, string> {
                { "error", errorText },
                { "description", errorDescription }
            }, cts);

        public async Task GetExternalIpAsync(CancellationTokenSource cts)
        {
            if (!ConnectionAlive)
                return;

            var res = await _webRequests.GetAsync("http://icanhazip.com", cts);
            _externalIp = res.Response;
        }

        public async Task<WebServerResponse> GetAsync(string path, CancellationTokenSource cts)
        {
            if (!ConnectionAlive)
                return new(Error.ConnectionError);

            return await _webRequests.GetAsync($"http://{Ip}/api/{path}", cts);
        }

        public async Task<WebServerResponse> PostAsync(string path, object data, CancellationTokenSource cts)
        {
            if (!ConnectionAlive)
                return new(Error.ConnectionError);

            return await _webRequests.PostAsync($"http://{Ip}/api/{path}", data, cts);
        }
    }
}