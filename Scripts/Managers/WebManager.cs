using Godot;

namespace GodotModules
{
    public class WebManager
    {
        public bool ConnectionAlive { get; private set; }

        private string _externalIp;
        private readonly WebRequests _webRequests;
        private readonly string _ip;
        private Task<WebServerResponse> _checkConnection;

        public WebManager(WebRequests webRequests, string ip)
        {
            _webRequests = webRequests;
            _ip = ip.IsAddress() ? ip : "localhost:4000";
        }

        public async Task CheckConnectionAsync()
        {
            if (_checkConnection != null && !_checkConnection.IsCompleted) // only one task checking the liveness of the connection should be active at a time
                return;

            _checkConnection = _webRequests.GetAsync($"http://{_ip}/api/ping");

            await _checkConnection;

            if (_checkConnection.Result.Error != Error.Ok)
                ConnectionAlive = false;
            else
                ConnectionAlive = true;
        }

        public async Task<WebServerResponse> RemoveLobbyPlayerAsync() => await PostAsync("servers/players/remove", DataExternalIp);
        public async Task<WebServerResponse> AddLobbyPlayerAsync() => await PostAsync("servers/players/add", DataExternalIp);
        public async Task<WebServerResponse> RemoveLobbyAsync() => await PostAsync("server/remove", DataExternalIp);
        /*public async Task<WebServerResponse> AddLobbyAsync() =>
            await PostAsync("servers/add", lobby)*/
        
        private Dictionary<string, string> DataExternalIp => new() { { "Ip", _externalIp } };

        public async Task<WebServerResponse> SendErrorAsync(string errorText, string errorDescription) =>
            await PostAsync("errors/post", new Dictionary<string, string> {
                { "error", errorText },
                { "description", errorDescription }
            });

        public async Task GetExternalIpAsync() 
        {
            if (!ConnectionAlive)
                return;
            
            var res = await _webRequests.GetAsync("http://icanhazip.com");
            _externalIp = res.Response;
        }

        public async Task<WebServerResponse> GetAsync(string path) 
        {
            if (!ConnectionAlive)
                return new WebServerResponse(Error.ConnectionError);

            return await _webRequests.GetAsync($"http://{_ip}/api/{path}");
        }

        public async Task<WebServerResponse> PostAsync(string path, object data) 
        {
            if (!ConnectionAlive)
                return new WebServerResponse(Error.ConnectionError);

            return await _webRequests.PostAsync($"http://{_ip}/api/{path}", data);
        }
    }
}