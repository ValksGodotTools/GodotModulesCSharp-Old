namespace GodotModules 
{
    public class TokenManager 
    {
        private Dictionary<string, CancellationTokenSource> _cts = new();

        public CancellationTokenSource Create(string name)
        {
            Cancel(name);
            _cts[name] = new();
            return _cts[name];
        }

        public bool Cancelled(string name) => _cts.ContainsKey(name) && _cts[name].IsCancellationRequested;

        public void Cancel(string name) 
        {
            if (_cts.ContainsKey(name)) 
                _cts[name].Cancel();
        }

        public void Cleanup()
        {
            _cts.Values.ForEach(x => {
                x.Cancel();
                x.Dispose();
            });
        }
    }
}