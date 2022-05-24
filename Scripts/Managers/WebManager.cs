namespace GodotModules
{
    public class WebManager
    {
        public string Ip { get; set; }
        public bool ConnectionAlive { get; private set; }

        private string _externalIp;

        private readonly WebRequests _webRequests;
        private readonly TokenManager _tokenManager;

        private Dictionary<string, string> DataExternalIp => new() { { "Ip", _externalIp } };

        public WebManager(WebRequests webRequests, TokenManager tokenManager, string ip)
        {
            _tokenManager = tokenManager;
            _webRequests = webRequests;
            Ip = ip.IsAddress() ? ip : "localhost:4000";
        }

        public async Task CheckConnectionAsync()
        {
            var res = await _webRequests.GetAsync($"http://{Ip}/api/ping", _tokenManager.Create("check_connection")); // by not doing GetAPIAsync or GetAsync we are skipping the bool ConnectionAlive check

            if (res == null)
                return;

            ConnectionAlive = res.Error == Error.Ok;
        }

        public async Task<WebServerResponse> RemoveLobbyPlayerAsync() =>
            await PostAPIAsync("servers/players/remove", DataExternalIp, "remove_lobby_player");

        public async Task<WebServerResponse> AddLobbyPlayerAsync() =>
            await PostAPIAsync("servers/players/add", DataExternalIp, "add_lobby_player");

        public async Task<WebServerResponse> RemoveLobbyAsync() =>
            await PostAPIAsync("server/remove", DataExternalIp, "remove_lobby");

        /*public async Task<WebServerResponse> AddLobbyAsync(CancellationTokenSource cts) =>
            await PostAPIAsync("servers/add", lobby)*/

        public async Task<WebServerResponse> SendErrorAsync(string errorText, string errorDescription) =>
            await PostAPIAsync("errors/post", new Dictionary<string, string> {
                { "error", errorText },
                { "description", errorDescription }
            }, "send_error");

        public async Task GetExternalIpAsync()
        {
            if (!ConnectionAlive)
                return;

            var res = await GetAsync("http://icanhazip.com", "get_external_ip");
            _externalIp = res.Response;
        }

        public async Task<WebServerResponse> GetAPIAsync(string path, string ctsName) =>
            await GetAsync($"http://{Ip}/api/{path}", ctsName);

        public async Task<WebServerResponse> PostAPIAsync(string path, object data, string ctsName) =>
            await PostAsync($"http://{Ip}/api/{path}", data, ctsName);

        private async Task<WebServerResponse> GetAsync(string path, string ctsName) =>
            ConnectionAlive ? await _webRequests.GetAsync(path, _tokenManager.Create(ctsName)) : new(Error.ConnectionError);

        private async Task<WebServerResponse> PostAsync(string path, object data, string ctsName) =>
            ConnectionAlive ? await _webRequests.PostAsync(path, data, _tokenManager.Create(ctsName)) : new(Error.ConnectionError);
    }
}