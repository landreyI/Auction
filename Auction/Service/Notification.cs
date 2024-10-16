using Auction.Models.DBModels;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace Auction.Service
{
    public class Notification
    {
        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly ConnectionMapping _connectionMapping;

        public Notification(IHubContext<AuctionHub> hubContext, ConnectionMapping connectionMapping)
        {
            _hubContext = hubContext;
            _connectionMapping = connectionMapping;
        }

        public async Task SendAuctionNewBidMessage(string idLot, string user, string bid)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNewBidMessage", idLot, user, bid);
        }

        public async Task ConnectionLotChat(int idLot, int idUser)
        {
            var connectionId = _connectionMapping.GetConnection(idUser.ToString());

            if (connectionId != null)
            {
                await _hubContext.Groups.AddToGroupAsync(connectionId, "LotChat-" + idLot);
            }
        }
        public async Task DisconnectedLotChat(int idLot, int idUser)
        {
            var connectionId = _connectionMapping.GetConnection(idUser.ToString());

            if (connectionId != null)
            {
                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, "LotChat-" + idLot);
            }
        }
        public async Task SendMessagePersonalChat(string userSenderId, string userRecipientId, string userSenderNick, string message, string fontSender, string fontRecipient)
        {
            var connectionSenderId = _connectionMapping.GetConnection(userSenderId);
            var connectionRecipientId = _connectionMapping.GetConnection(userRecipientId);

            if (connectionSenderId != null)
            {
                await _hubContext.Clients.Client(connectionSenderId).SendAsync("ReceiveMessageFromUserChat", userSenderNick, message, fontSender);
            }

            if (connectionRecipientId != null)
            {
                await _hubContext.Clients.Client(connectionRecipientId).SendAsync("ReceiveMessageFromUserChat", userSenderNick, message, fontRecipient);
            }
        }

        public async Task SendSystemAlertNotification(int userId, string message)
        {
            var connectionId = _connectionMapping.GetConnection(userId.ToString());
            if (connectionId != null)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveSystemAlert", message);
            }
        }
        public async Task DeletePersonalChat(string idPlayer1, string idPlayer2, int idChat)
        {
            var connectionIdPlayer1 = _connectionMapping.GetConnection(idPlayer1);
            var connectionIdPlayer2 = _connectionMapping.GetConnection(idPlayer2);

            if (connectionIdPlayer1 != null)
            {
                await _hubContext.Clients.Client(connectionIdPlayer1).SendAsync("DeletePersonalChat", idChat);
            }

            if (connectionIdPlayer2 != null)
            {
                await _hubContext.Clients.Client(connectionIdPlayer2).SendAsync("DeletePersonalChat", idChat);
            }
        }
    }
}
