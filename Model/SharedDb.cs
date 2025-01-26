using System.Collections.Concurrent;

namespace RealTimeCommunication_SignalR.Model
{
    public class SharedDb
    {
        private readonly ConcurrentDictionary<string, UserConnection> _connections;

        public SharedDb()
        {
            _connections = new ConcurrentDictionary<string, UserConnection>();
        }

        public ConcurrentDictionary<string, UserConnection> Connection => _connections;

    }
}
