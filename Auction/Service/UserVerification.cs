using Auction.Models.DBModels;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Auction.Service
{
    public class UserVerification
    {
        public static ClaimsPrincipal AuthorizationUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

            return new ClaimsPrincipal(claimsIdentity);
        }
        public static AuthenticationProperties AuthenProperties()
        {
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(3),
            };
            return authProperties;
        }
        public static int? checAuthorization(HttpContext httpContext)
        {
            string idUserString = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(idUserString, out int idUser))
            {
                return idUser;
            }
            else
            {
                return null;
            }
        }
        public static ClaimsPrincipal UpdateUserRole(ClaimsPrincipal currentPrincipal, string newRole)
        {
            // Проверяем, что текущий ClaimsPrincipal содержит утверждение NameIdentifier
            var userClaim = currentPrincipal.FindFirst(ClaimTypes.NameIdentifier);

            if (userClaim != null)
            {
                // Создаем новое утверждение с обновленной ролью
                var updatedRoleClaim = new Claim(ClaimTypes.Role, newRole);

                // Создаем новый список утверждений
                var newClaims = currentPrincipal.Claims
                    .Where(claim => claim.Type != ClaimTypes.Role)
                    .ToList();

                newClaims.Add(updatedRoleClaim);

                // Создаем новый объект ClaimsIdentity с обновленным списком утверждений
                var newIdentity = new ClaimsIdentity(newClaims, currentPrincipal.Identity.AuthenticationType);

                // Создаем новый объект ClaimsPrincipal с обновленным объектом ClaimsIdentity
                var newPrincipal = new ClaimsPrincipal(newIdentity);

                return newPrincipal;
            }

            return currentPrincipal;
        }
    }
}
