using Godot;

namespace GodotModules.Netcode 
{
    public class WebRequests
    {
        private readonly Node _requests;

        public WebRequests(Node requests)
        {
            _requests = requests;
        }

        public async Task<WebServerResponse> GetAsync(string url, CancellationTokenSource cts)
        {
            var webRequest = new WebRequest(_requests);
            return await webRequest.GetAsync(url, cts);
        }

        public async Task<WebServerResponse> PostAsync(string url, object data, CancellationTokenSource cts)
        {
            var webRequest = new WebRequest(_requests);
            return await webRequest.PostAsync(url, data, cts);
        }
    }
}