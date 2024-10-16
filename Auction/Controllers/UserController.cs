using Auction.Models.DBModels;
using Auction.Service;
using Auction.Service.DBServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auction.Controllers
{

    public class UserController : Controller
    {
        private readonly DBService dbService;
        private readonly Notification _notification;
        private readonly AutoBidStorage _autoBidStorage;
        public UserController(ILogger<HomeController> logger, DBService _bdService, Notification notification, AutoBidStorage autoBidStorage)
        {
            dbService = _bdService;
            _notification = notification;
            _autoBidStorage = autoBidStorage;
        }

        public async Task<IActionResult> MyPage(int? UserId)
        {
            if (UserId != null)
            {
                if (User.Identity.IsAuthenticated)
                    if (UserId == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
                        ViewBag.CheckAccount = true;


                var autoBidstoUser = _autoBidStorage.GetAutoBidsToUser(UserId.Value);

                var selectAutoBids = await Task.WhenAll(autoBidstoUser.Select(async auto => new
                {
                    LotAutoBid = await dbService.AuctionService.GetLot(auto.idLot),
                    MaxBid = auto.maxBid,
                    RateBid = auto.rateBid,
                }));

                ViewBag.AutoBidsUser = selectAutoBids;
            }

            return View(await dbService.UserService.GetUserAsync(UserId));
        }

        [Authorize]
        public async Task<IActionResult> MyMessage()
        {
            int idUser = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            List<Chat> chats = await dbService.ChatService.GetChatsByPlayerIdAsync(idUser);

            ViewBag.UserId = idUser;

            return View(chats);
        }

        [Authorize]
        public async Task<IActionResult> PersonalChat(int? idChat, int? recipientId)
        {
            Chat chatModel = null;

            if (idChat != null)
            {
                chatModel = await dbService.ChatService.GetChatAsync(idChat.Value);
            }

            else if (recipientId != null)
            {
                int? idChatFind = await dbService.ChatService.AddChatAsync(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)), recipientId);

                chatModel = await dbService.ChatService.GetChatAsync(idChatFind.Value);
            }

            ViewBag.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            ViewBag.UserName = User.FindFirstValue(ClaimTypes.Name);

            return View(chatModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SendMessage(int chatId, string senderNick, string senderId, string recipientId, string message)
        {
            if (await dbService.ChatService.AddMessageAsync(chatId, int.Parse(senderId), message))
            {
                string fontColor;
                if (User.FindFirstValue(ClaimTypes.NameIdentifier) == senderId) fontColor = "primary";
                else fontColor = "success";

                await _notification.SendMessagePersonalChat(senderId, recipientId, senderNick, message, "primary", "success");

                await _notification.SendSystemAlertNotification(int.Parse(recipientId), "You have received a new message!");
            }
            else
            {
                return Json(new { success = false, errorMessage = "Error sending message" });
            }
            return Json(new { success = true });
        }

        public async Task<IActionResult> DeleteChat(int idChat)
        {
            Chat chatModel = await dbService.ChatService.GetChatAsync(idChat);

            if (chatModel == null || !await dbService.ChatService.DeleteChatAsync(idChat))
            {
                return Json(new { success = false, errorMessage = "There was an error deleting the chat" });
            }

            await _notification.DeletePersonalChat(chatModel.IdUser1.ToString(), chatModel.IdUser2.ToString(), idChat);

            return Json(new { success = true });
        }

        [Authorize]
        public async Task<IActionResult> DeleteAutoBid(int? idLot)
        {
            _autoBidStorage.RemoveAutoBid(idLot.Value, int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));

            return Json(new { success = true });
        }
    }
}