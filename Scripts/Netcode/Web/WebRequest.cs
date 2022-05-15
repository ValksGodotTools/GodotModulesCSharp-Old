using Godot;

namespace GodotModules 
{
    public class WebRequest : HTTPRequest
    {
        private static int _id;
        private WebServerResponse _webServerResponse;

        public WebRequest(Node parent)
        {
            Timeout = 10; // in seconds
            UseThreads = true;
            parent.AddChild(this);
            var error = Connect("request_completed", this, nameof(OnRequestCompleted));
            if (error != Error.Ok)
                GM.LogWarning("Failed to connect request_completed signal for WebRequest");
        }

        public async Task<WebServerResponse> GetAsync(string url)
        {
            Name = $"Request GET ({++_id})";

            var errorRequest = Request(url);

            if (errorRequest != Error.Ok)
            {
                //GM.LogWarning($"Failed to make GET request to {url}");
                return new WebServerResponse(errorRequest);
            }

            await ToSignal(this, "request_completed");
            
            _id--;
            QueueFree();
            return _webServerResponse;
        }

        public async Task<WebServerResponse> PostAsync(string url, object data)
        {
            Name = $"Request POST ({++_id})";

            var query = JSON.Print(data);
            var headers = new string[] { "Content-Type: application/json" };
            var errorRequest = Request(url, headers, true, HTTPClient.Method.Post, query);

            if (errorRequest != Error.Ok)
            {
                //GM.LogWarning($"Failed to make GET request to {url}");
                return new WebServerResponse(errorRequest);
            }

            await ToSignal(this, "request_completed");

            _id--;
            QueueFree();
            return _webServerResponse;
        }

        public void OnRequestCompleted(int result, int response_code, string[] headers, byte[] body)
        {
            var godotError = (Error)result;
            var webError = (System.Net.HttpStatusCode)response_code;
            if (godotError != Error.Ok || webError != System.Net.HttpStatusCode.OK)
            {
                if (godotError == Error.FileCantOpen) // if request times out godot throws a FileCantOpen error, this seems weird
                    godotError = Error.ConnectionError;

                _webServerResponse = new WebServerResponse(godotError);
                return;
            }

            _webServerResponse = new WebServerResponse(godotError, System.Text.Encoding.UTF8.GetString(body));
        }
    }

    public class WebServerResponse 
    {
        public Error Error { get; set; }
        public string Response { get; set; }

        public WebServerResponse(Error error, string response = null)
        {
            Error = error;
            Response = response;
        }
    }
}