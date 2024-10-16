using Auction.Data;
using Auction.Models.DBModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Auction.Service.DBServices
{
    public class DBPurchaseService
    {
        private readonly AppDBContext _dbContext;
        private readonly DBChatService _chatService;
        private readonly ILogger<DBService> _logger;
        private readonly CloudinaryService _cloudinaryService;
        private readonly Notification _notification;
        public DBPurchaseService(AppDBContext dbContext, ILogger<DBService> logger, CloudinaryService cloudinaryService, DBChatService chatService, Notification notification)
        {
            _dbContext = dbContext;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
            _chatService = chatService;
            _notification = notification;
        }

        public async Task AddingPurchases()
        {
            try
            {
                List<Lot> lotsToAdding = await _dbContext.Lots
                    .Where(lot => lot.EndTime < DateTime.Now && lot.UserIdBid != null && !_dbContext.Purchases.Any(p => p.LotId == lot.LotId))
                    .ToListAsync();

                if (lotsToAdding.Any())
                {
                    List<Purchase> purchases = new List<Purchase>();
                    foreach(var lot in lotsToAdding)
                    {
                        purchases.Add(new Purchase { 
                            BuyerId = lot.UserIdBid.Value, 
                            LotId = lot.LotId, 
                            FinalPrice = lot.CurrentPrice.Value, 
                            PurchaseDate = DateTime.Now 
                        });
                        int? idChat = await _chatService.AddChatAsync(lot.UserId, lot.UserIdBid);

                        string messageWin = $"Congratulations, you won my item in the auction - {lot.Title}, for ${lot.CurrentPrice}";

                        if (idChat != null)
                            await _chatService.AddMessageAsync(idChat.Value, lot.UserId, messageWin);

                        await _notification.SendSystemAlertNotification(lot.UserIdBid.Value, messageWin);
                    }

                    await _dbContext.Purchases.AddRangeAsync(purchases);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when adding expired lots.");
            }
        }
        public async Task DeletingExpiredLots()
        {
            try
            {
                // Находим все лоты, которые просрочены и не имеют записи в таблице Purchase
                List<Lot> lotsToRemove = await _dbContext.Lots
                    .Where(lot => lot.EndTime < DateTime.Now && lot.UserIdBid == null && !_dbContext.Purchases.Any(p => p.LotId == lot.LotId))
                    .ToListAsync();

                if (lotsToRemove.Any())
                {
                    foreach (var lot in lotsToRemove)
                    {
                        var deletionResults = await _cloudinaryService.DeleteImagesLotAsync(lot);

                        foreach (var deletionResult in deletionResults)
                        {
                            if (deletionResult.Result != "ok")
                            {
                                Console.WriteLine("Error when deleting image from cloudinary.");
                            }
                        }
                    }

                    _dbContext.Lots.RemoveRange(lotsToRemove);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when deleting expired lots.");
            }
        }
    }
}
