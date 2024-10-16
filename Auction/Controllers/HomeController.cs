using Auction.Models;
using Auction.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Auction.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DBService dbService;

        public HomeController(ILogger<HomeController> logger, DBService _bdService)
        {
            _logger = logger;
            dbService = _bdService;
        }

        public IActionResult Home()
        {

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult NoAuthorize()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
