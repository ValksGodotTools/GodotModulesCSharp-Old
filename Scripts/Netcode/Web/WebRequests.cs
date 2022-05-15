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

        public async Task<WebServerResponse> GetAsync(string url)
        {
            var webRequest = new WebRequest(_requests);
            return await webRequest.GetAsync(url);
        }

        public async Task<WebServerResponse> PostAsync(string url, object data)
        {
            var webRequest = new WebRequest(_requests);
            return await webRequest.PostAsync(url, data);
        }
    }
}