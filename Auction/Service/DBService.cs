using Auction.Data;
using Auction.Service.DBServices;

namespace Auction.Service
{
    public class DBService
    {
        public DBUserService UserService { get; set; }
        public DBAuctionService AuctionService { get; set; }
        public DBPurchaseService PurchaseService { get; set; }
        public DBChatService ChatService { get; set; }

        private readonly AppDBContext _dbContext;
        private readonly ILogger<DBService> _logger;
        public DBService(AppDBContext dbContext, ILogger<DBService> logger, DBUserService dBUserService, DBAuctionService dBAuctionService, DBPurchaseService dBpurchaseService, DBChatService dBchatService)
        {
            UserService = dBUserService;
            AuctionService = dBAuctionService;
            PurchaseService = dBpurchaseService;
            ChatService = dBchatService;

            _dbContext = dbContext;
            _logger = logger;
        }
    }
}
