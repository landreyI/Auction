using Auction.Data;
using Auction.Models;
using Auction.Models.DBModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Auction.Service.DBServices
{
    public class DBAuctionService
    {
        private readonly AppDBContext _dbContext;
        private readonly ILogger<DBService> _logger;
        public DBAuctionService(AppDBContext dbContext, ILogger<DBService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Lot> GetLot(int? idLot)
        {
            if (idLot == null) return null;

            try
            {
                return await _dbContext.Lots
                    .Include(p => p.Purchases)
                    .ThenInclude(p => p.Buyer)
                    .Include(p => p.ImgLots)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.LotId == idLot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error when accessing Lots");
                return null;
            }
        }

        public async Task<List<Lot>> GetMyLots(int? idUser)
        {
            if (idUser == null) return null;

            try
            {
                return await _dbContext.Lots
                    .Where(p => p.UserId == idUser)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error when accessing Lots");
                return null;
            }
        }


        public async Task<bool> NewBidLot(int? idLot, int? newBid, int? idUser)
        {
            if (idLot == null || newBid == null || idUser == null) return false;

            try
            {
                Lot lot = await _dbContext.Lots.FirstOrDefaultAsync(l => l.LotId == idLot);

                if(lot == null) return false;

                lot.CurrentPrice = newBid;
                lot.TotalBids = (lot.TotalBids ?? 0) + 1;
                lot.UserIdBid = idUser;

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error when changing lot rate");
                return false;
            }
        }

        public async Task<bool> IncrementView(int? idLot)
        {
            if (idLot == null) return false;

            try
            {
                var lot = await _dbContext.Lots.FirstOrDefaultAsync(l => l.LotId == idLot);

                if (lot == null)
                {
                    _logger.LogWarning("Lot with id {idLot} not found.", idLot);
                    return false;
                }

                lot.TotalViews = (lot.TotalViews ?? 0) + 1;

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when accessing Lots for id {idLot}", idLot);
                return false;
            }
        }


        public async Task<List<Lot>> GetAllLots(LotFilterParams filterParams)
        {
            try
            {
                IQueryable<Lot> query = _dbContext.Lots
                    .Include(p => p.ImgLots)
                    .Include(p => p.User);

                if (filterParams.IsPopular.HasValue && filterParams.IsPopular.Value)
                {
                    query = query.OrderBy(lot => lot.TotalViews);
                }

                if (filterParams.IdLastLot.HasValue)
                {
                    query = query.Where(l => l.LotId > filterParams.IdLastLot.Value);
                }

                if (filterParams.DateTime.HasValue)
                {
                    query = query.Where(lot => lot.EndTime > DateTime.Now);
                }

                if (filterParams.Price.HasValue)
                {
                    query = query.Where(lot => lot.CurrentPrice < filterParams.Price.Value);
                }

                if (!string.IsNullOrEmpty(filterParams.Location))
                {
                    query = query.Where(lot => lot.Location.ToLower().Contains(filterParams.Location.ToLower()));
                }

                query = query.OrderBy(l => l.LotId)
                     .Take(filterParams.Size ?? 20);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when accessing Lots}");
                return null;
            }
        }
        public async Task<bool> AddLot(Lot lot)
        {
            if(lot == null) return false;
            try
            {
                if (lot.CurrentPrice == null) lot.CurrentPrice = lot.StartPrice;

                await _dbContext.Lots.AddAsync(lot);

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error when adding Lots");
                return false;
            }
        }

        public async Task<bool> UpdateLot(Lot lot)
        {
            if (lot == null) return false;
            try
            {
                _dbContext.Lots.Update(lot);

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when updating Lots");
                return false;
            }
        }

        public async Task<bool> AddImgLot(ImgLot imgLot)
        {
            if (imgLot == null) return false;
            try
            {
                await _dbContext.ImgLots.AddAsync(imgLot);

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error when adding ImgLots");
                return false;
            }
        }
        public async Task<bool> AddImgLots(List<ImgLot> imgLots)
        {
            if (imgLots == null || !imgLots.Any()) return false;

            try
            {
                await _dbContext.ImgLots.AddRangeAsync(imgLots);

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when adding ImgLots");
                return false;
            }
        }

        public async Task<bool> DelImgLot(int? idImgLot)
        {
            if (idImgLot == null) return false;
            try
            {
                var imgLot =  await _dbContext.ImgLots.FirstOrDefaultAsync(i => i.ImgLotId == idImgLot);

                if(imgLot == null) return false;

                _dbContext.ImgLots.Remove(imgLot);

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error when delete ImgLots");
                return false;
            }
        }
        public async Task<bool> DelImgLot(ImgLot? imgLot)
        {
            if (imgLot == null) return false;
            try
            {
                _dbContext.ImgLots.Remove(imgLot);

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error when delete ImgLots");
                return false;
            }
        }

        public async Task<bool> DelImgLots(List<ImgLot>? imgLots)
        {
            if (imgLots == null || imgLots.Count == 0) return false;

            try
            {
                _dbContext.ImgLots.RemoveRange(imgLots);

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error when deleting ImgLots");
                return false;
            }
        }

        public async Task<bool> DeleteLot(int? idLot)
        {
            if (idLot == null) return false;

            try
            {
                var lot = await _dbContext.Lots
                                          .Include(l => l.ImgLots)
                                          .FirstOrDefaultAsync(l => l.LotId == idLot);

                if (lot == null) return false;

                if (lot.ImgLots != null && lot.ImgLots.Any())
                {
                    _dbContext.ImgLots.RemoveRange(lot.ImgLots);
                }

                _dbContext.Lots.Remove(lot);

                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when accessing Lots for id {idLot}", idLot);
                return false;
            }
        }

    }
}
