namespace Auction.Service
{
    public class DBCheckService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DBCheckService> _logger;
        public DBCheckService(IServiceProvider serviceProvider, ILogger<DBCheckService> logger)
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

                        await CheckDatabaseAsync(dbContext);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking the database.");
                }

                // Ждем 10 минут перед следующей проверкой
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task CheckDatabaseAsync(DBService dbContext)
        {
            await dbContext.PurchaseService.DeletingExpiredLots();
            await dbContext.PurchaseService.AddingPurchases();
        }
    }
}
