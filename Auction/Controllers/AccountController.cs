using Auction.Models.DBModels;
using Auction.Models.ViewModels;
using Auction.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auction.Controllers
{
    public class AccountController : Controller
    {
        private readonly DBService dbService;
        public AccountController(DBService _bdService)
        {
            dbService = _bdService;
        }

        [HttpPost]
        public async Task<IActionResult> Registration(CreateUserView user)
        {
            string errorMessage = "";

            if (!ModelState.IsValid)
            {
                errorMessage = ModelState.Values.SelectMany(v => v.Errors)
                                  .FirstOrDefault()?.ErrorMessage;

            }
            else
            {
                if (!await dbService.UserService.CheckEmailAsync(user.Email) && !await dbService.UserService.CheckUserNameAsync(user.UserName))
                {
                    User userModel = new User
                    {
                        UserName = user.UserName,
                        Email = user.Email,
                        Password = user.Password,
                        CreatedAt = DateTime.Now
                    };
                    if (await dbService.UserService.AddUserAsync(userModel))
                    {
                        await HttpContext.SignInAsync("Cookies", UserVerification.AuthorizationUser(userModel), UserVerification.AuthenProperties());

                        return Json(new { success = true });
                    }
                    errorMessage = "Error when adding Player to table DB";
                }
                else errorMessage = "A player with this nickname or email is already taken";
            }

            return Json(new { success = false, errorMessage = errorMessage });
        }

        [HttpPost]
        public async Task<IActionResult> Authorization(string Email, string Password)
        {
            string errorMessage = "";

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                errorMessage = "Please fill in all required fields.";
            }
            else
            {
                if (await dbService.UserService.CheckEmailAsync(Email))
                {
                    var user = await dbService.UserService.GetUserAsync(Email);
                    if (user != null && Password == user.Password)
                    {
                        await HttpContext.SignInAsync("Cookies", UserVerification.AuthorizationUser(user), UserVerification.AuthenProperties());

                        return Json(new { success = true });
                    }
                    errorMessage = "Incorrect password";
                }
                else errorMessage = "There is no player with this email";
            }

            return Json(new { success = false, errorMessage = errorMessage });
        }
        public async Task<IActionResult> Clouse()
        {
            await HttpContext.SignOutAsync("Cookies");

            return RedirectToAction("Home", "Home");
        }

    }
}
