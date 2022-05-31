namespace GodotModules
{
    public static class Notifications
    {
        private static Dictionary<string, List<Listener>> _listeners = new();

        public static void AddListener(Node sender, string eventType, Action<Node, object[]> action)
        {
            if (!_listeners.ContainsKey(eventType))
                _listeners.Add(eventType, new List<Listener>());

            foreach (var pair in _listeners)
                foreach (var listener in pair.Value)
                    if (sender.GetInstanceId() == listener.Id)
                        throw new InvalidOperationException($"Tried to add duplicate listener of event type '{eventType}'");

            _listeners[eventType].Add(new Listener(sender.GetInstanceId(), action));
        }

        public static void RemoveListener(Node sender, string eventType)
        {
            if (!_listeners.ContainsKey(eventType))
                throw new InvalidOperationException($"Tried to remove listener of event type '{eventType}' from an event type that has not even been defined yet");

            var found = false;

            foreach (var pair in _listeners)
                for (int i = pair.Value.Count - 1; i >= 0; i--)
                    if (sender.GetInstanceId() == pair.Value[i].Id) 
                    {
                        found = true;
                        pair.Value.RemoveAt(i);
                    }

            if (!found)
                throw new InvalidOperationException($"Tried to remove non-existent event type '{eventType}' from listeners");
        }

        public static void Notify(Node sender, string eventType, params object[] args)
        {
            foreach (var pair in _listeners)
                foreach (var listener in pair.Value)
                    listener.Action(sender, args);
        }

        private class Listener 
        {
            public ulong Id { get; set; }
            public Action<Node, object[]> Action { get; set; }

            public Listener(ulong id, Action<Node, object[]> action)
            {
                Id = id;
                Action = action;
            }
        }
    }
}