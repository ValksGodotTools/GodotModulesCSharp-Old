using Godot;

namespace GodotModules
{
    public class WebManager
    {
        public string Ip { get; set; }
        public bool ConnectionAlive { get; private set; }

        private string _externalIp;

        private Dictionary<string, CancellationTokenSource> _cts = new();

        private readonly WebRequests _webRequests;

        public WebManager(WebRequests webRequests, string ip)
        {
            _webRequests = webRequests;
            Ip = ip.IsAddress() ? ip : "localhost:4000";
        }

        public async Task CheckConnectionAsync()
        {
            var taskName = "check_connection";
            if (_cts.ContainsKey(taskName)) 
            {
                _cts[taskName].Cancel();
                _cts[taskName].Dispose();
            }
            _cts[taskName] = new();
            var res = await _webRequests.GetAsync($"http://{Ip}/api/ping", _cts[taskName]);

            if (res == null)
                return;

            if (res.Error != Error.Ok)
                ConnectionAlive = false;
            else
                ConnectionAlive = true;
        }

        public async Task<WebServerResponse> RemoveLobbyPlayerAsync() => await PostAsync("servers/players/remove", DataExternalIp, "remove_lobby_player");
        public async Task<WebServerResponse> AddLobbyPlayerAsync() => await PostAsync("servers/players/add", DataExternalIp, "add_lobby_player");
        public async Task<WebServerResponse> RemoveLobbyAsync() => await PostAsync("server/remove", DataExternalIp, "remove_lobby");
        /*public async Task<WebServerResponse> AddLobbyAsync() =>
            await PostAsync("servers/add", lobby)*/

        private Dictionary<string, string> DataExternalIp => new() { { "Ip", _externalIp } };

        public async Task<WebServerResponse> SendErrorAsync(string errorText, string errorDescription) => 
            await PostAsync("errors/post", new Dictionary<string, string> {
                { "error", errorText },
                { "description", errorDescription }
            }, "send_error");

        public async Task GetExternalIpAsync()
        {
            if (!ConnectionAlive)
                return;

            var taskName = "get_external_ip";
            if (_cts.ContainsKey(taskName)) 
            {
                _cts[taskName].Cancel();
                _cts[taskName].Dispose();
            }
            _cts[taskName] = new();
            var res = await _webRequests.GetAsync("http://icanhazip.com", _cts[taskName]);
            _externalIp = res.Response;
        }

        public async Task<WebServerResponse> GetAsync(string path, string taskName)
        {
            if (!ConnectionAlive)
                return new(Error.ConnectionError);

            if (_cts.ContainsKey(taskName)) 
            {
                _cts[taskName].Cancel();
                _cts[taskName].Dispose();
            }
            _cts[taskName] = new();

            return await _webRequests.GetAsync($"http://{Ip}/api/{path}", _cts[taskName]);
        }

        public async Task<WebServerResponse> PostAsync(string path, object data, string taskName)
        {
            if (!ConnectionAlive)
                return new(Error.ConnectionError);

            if (_cts.ContainsKey(taskName)) 
            {
                _cts[taskName].Cancel();
                _cts[taskName].Dispose();
            }
            _cts[taskName] = new();

            return await _webRequests.PostAsync($"http://{Ip}/api/{path}", data, _cts[taskName]);
        }

        public void Cleanup()
        {
            _cts.ForEach(x => {
                var cts = x.Value;
                cts.Cancel();
                cts.Dispose();
            });
        }
    }
}