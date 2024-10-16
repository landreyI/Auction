namespace Auction.Service
{
    public class ConnectionMapping
    {
        private readonly Dictionary<string, string> _connections = new Dictionary<string, string>();

        public void AddConnection(string userId, string connectionId)
        {
            lock (_connections)
            {
                _connections[userId] = connectionId;
            }
        }

        public void RemoveConnection(string userId)
        {
            lock (_connections)
            {
                _connections.Remove(userId);
            }
        }

        public string GetConnection(string userId)
        {
            lock (_connections)
            {
                _connections.TryGetValue(userId, out var connectionId);
                return connectionId;
            }
        }

    }
}
