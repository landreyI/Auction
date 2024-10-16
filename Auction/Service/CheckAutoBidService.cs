using Auction.Models.DBModels;
using System.Security.Claims;

namespace Auction.Service
{
    public class CheckAutoBidService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CheckAutoBidService> _logger;
        public CheckAutoBidService(IServiceProvider serviceProvider, ILogger<CheckAutoBidService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Создаем новый скоуп для получения необходимых сервисов
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DBService>();
                        var autoBidStorageContext = scope.ServiceProvider.GetRequiredService<AutoBidStorage>();
                        var notification = scope.ServiceProvider.GetRequiredService<Notification>();

                        await CheckAutoBidsAsync(dbContext, autoBidStorageContext, notification);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking the database.");
                }

                // Ждем 1 минут перед следующей проверкой
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task CheckAutoBidsAsync(DBService dbContext, AutoBidStorage autoBidStorage, Notification notification)
        {
            var autoBids = autoBidStorage.GetAutoBids();
            var tasks = new List<Task>();

            foreach (var autoBid in autoBids.ToList())  // Используем ToList() для создания копии списка
            {
                tasks.Add(ProcessAutoBidAsync(dbContext, autoBidStorage, notification, autoBid));
            }

            await Task.WhenAll(tasks);  // Выполняем все задачи параллельно
        }

        private async Task ProcessAutoBidAsync(DBService dbContext, AutoBidStorage autoBidStorage, Notification notification, AutoBid autoBid)
        {
            var lot = await dbContext.AuctionService.GetLot(autoBid.idLot);

            // Проверка существования лота и что ставка принадлежит не текущему пользователю
            if (lot != null && lot.UserIdBid != autoBid.idUser && lot.EndTime > DateTime.Now)
            {
                if (autoBid.maxBid <= lot.CurrentPrice)
                {

                    await notification.SendSystemAlertNotification(autoBid.idUser, "Your automatic bid for the item “car” has reached its maximum.");

                    autoBidStorage.RemoveAutoBid(autoBid);  // Удаляем ставку из autoBidStorage
                    return;
                }

                var newBidPrice = (int?)(lot.CurrentPrice) + autoBid.rateBid;

                if (newBidPrice > autoBid.maxBid)
                {
                    newBidPrice = autoBid.maxBid;
                }

                // Проверка успешности ставки
                if (!await dbContext.AuctionService.NewBidLot(lot.LotId, newBidPrice, autoBid.idUser))
                {
                    _logger.LogError("Failed to place new bid on Lot {0} by user {1}.", lot.LotId, autoBid.idUser);
                    return;
                }

                var user = await dbContext.UserService.GetUserAsync(autoBid.idUser);
                if (user != null)
                {
                    await notification.SendAuctionNewBidMessage(lot.LotId.ToString(), user.UserName, lot.CurrentPrice.ToString());
                }
            }
            else if (lot == null)
            {
                _logger.LogError("Lot {0} not found for AutoBid by user {1}.", autoBid.idLot, autoBid.idUser);
            }
            else if(lot.EndTime < DateTime.Now)
            {
                autoBidStorage.RemoveAutoBidsToLot(autoBid.idLot);
            }
        }

    }
}
