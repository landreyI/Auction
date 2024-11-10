using Microsoft.AspNetCore.SignalR;

namespace Auction.Service
{
    public class AuctionHub : Hub
    {
        private readonly ConnectionMapping _connectionMapping;

        public AuctionHub(ConnectionMapping connectionMapping)
        {
            _connectionMapping = connectionMapping;
        }
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                _connectionMapping.AddConnection(userId, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                _connectionMapping.RemoveConnection(userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinGroupLotChat(string lotId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "LotChat-" + lotId);
        }

        public async Task LeaveGroupLotChat(string lotId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "LotChat-" + lotId);
        }

        public async Task SendMessageToGroupLot(string lotId, string nickname, string message)
        {
            await Clients.Group("LotChat-" + lotId).SendAsync("ReceiveMessageGroupLot", nickname, message);
        }
    }
}
