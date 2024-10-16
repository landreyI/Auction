using Auction.Data;
using Auction.Models.DBModels;
using Microsoft.EntityFrameworkCore;

namespace Auction.Service.DBServices
{
    public class DBUserService
    {
        private readonly AppDBContext _dbContext;
        private readonly ILogger<DBService> _logger;
        public DBUserService(AppDBContext dbContext, ILogger<DBService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<bool> AddUserAsync(User? user)
        {
            if (user == null) return false;
            try
            {
                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error when adding User to table");
                return false;
            }
        }
        public async Task<bool> CheckEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;

            try
            {
                var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser == null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error when accessing User");
                return false;
            }
        }
        public async Task<bool> CheckUserNameAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return false;

            try
            {
                var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
                if (existingUser == null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error when accessing User");
                return false;
            }
        }
        public async Task<User> GetUserAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return null;

            try
            {
                return await _dbContext.Users
                    .Include(u => u.Lots)
                    .Include(u => u.Purchases)
                    .FirstOrDefaultAsync(p => p.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error when accessing User");
                return null;
            }
        }
        public async Task<User> GetUserAsync(int? idUser)
        {
            if (idUser == null) return null;

            try
            {
                return await _dbContext.Users
                    .Include(u => u.Lots)
                    .Include(u => u.Purchases)
                    .ThenInclude(u => u.Lot)
                    .FirstOrDefaultAsync(p => p.UserId == idUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error when accessing User");
                return null;
            }
        }
    }
}
