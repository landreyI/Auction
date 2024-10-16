using Auction.Models.DBModels;
using Auction.Models.ViewModels;
using Auction.Service;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Auction.Models;
using static Azure.Core.HttpHeader;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AuctionController.Controllers
{
    public class AuctionController : Controller
    {
        private readonly DBService dbService;
        private readonly CloudinaryService _cloudinaryService;
        private readonly Notification _notification;
        private readonly AutoBidStorage _autoBidStorage;
        public AuctionController(DBService _bdService, CloudinaryService cloudinaryService , Notification notification, AutoBidStorage autoBidStorage)
        {
            dbService = _bdService;
            _cloudinaryService = cloudinaryService;
            _notification = notification;
            _autoBidStorage = autoBidStorage;
        }

        [Route("Auction/InfoLot/{idLot?}")]
        public async Task<IActionResult> InfoLot(int ?idLot)
        {

            await dbService.AuctionService.IncrementView(idLot);
            Lot lot = await dbService.AuctionService.GetLot(idLot);

            return View(lot);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetMyLots()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var lots = await dbService.AuctionService.GetMyLots(userId);

            var myLots = lots.Select(l => new
            {
                LotId = l.LotId,
                Title = l.Title,
                CurrentBid = l.CurrentPrice
            });

            return Json(new { success = true, myLots = myLots });
        }

        [HttpPost]
        public async Task<IActionResult> NewBidLot(int? idLot, int? newBid)
        {
            if(!User.Identity.IsAuthenticated)
                return Json(new { success = false, errorMessage = "To place bets, you need to register or log in to your account!" });

            if (idLot == null) 
                return Json(new { success = false, errorMessage = "Error, item not found." });

            if (newBid == null)
                return Json(new { success = false, errorMessage = "Select a new bid!" });

            Lot lot = await dbService.AuctionService.GetLot(idLot);

            if(newBid <= lot.CurrentPrice) return Json(new { success = false, errorMessage = "The bid must be higher than the previous one!" });

            if (lot.EndTime <= DateTime.Now) return Json(new { success = false, errorMessage = "Time for betting is over!" });

            if (!await dbService.AuctionService.NewBidLot(idLot, newBid, int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))))
                return Json(new { success = false, errorMessage = "Select a new bid!" });

            string userName = User.FindFirstValue(ClaimTypes.Name);

            await _notification.SendAuctionNewBidMessage(idLot.ToString(), userName, newBid.ToString());

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> AutoBid(int? idLot, int? maxBid, int? rateBid)
        {
            if (!User.Identity.IsAuthenticated)
                return Json(new { success = false, errorMessage = "To place bets, you need to register or log in to your account!" });

            if (!idLot.HasValue)
                return Json(new { success = false, errorMessage = "Error, item not found." });


            if (!maxBid.HasValue || !rateBid.HasValue)
                return Json(new { success = false, errorMessage = "Fill in all fields!" });

            Lot lot = await dbService.AuctionService.GetLot(idLot);

            if (lot.EndTime <= DateTime.Now) 
                return Json(new { success = false, errorMessage = "Time for betting is over!" });

            if (maxBid <= lot.CurrentPrice) 
                return Json(new { success = false, errorMessage = "The bid must be higher than the previous one!" });

            if (rateBid > maxBid)
                return Json(new { success = false, errorMessage = "Rate bid cannot be higher than the maximum!" });

            int idUser = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if(! _autoBidStorage.AddAutoBid(idLot.Value, idUser, maxBid.Value, rateBid.Value))
                return Json(new { success = false, errorMessage = "An error occurred while processing the request!" });

            return Json(new { success = true });
        }

        public async Task<IActionResult> GetAllLots()
        {
            var filterParams = new LotFilterParams
            {
                Size = 20,
                DateTime = DateTime.Now
            };

            return View(await dbService.AuctionService.GetAllLots(filterParams));
        }

        [HttpPost]
        public async Task<IActionResult> LoadNewLots(int? idLastLot, DateTime? dateTime, int? price, string? location, bool? isPopular)
        {
            if(dateTime == null) dateTime = DateTime.Now;

            var filterParams = new LotFilterParams
            {
                Size = 20,
                DateTime = dateTime,
                Price = price,
                Location = location,
                IdLastLot = idLastLot,
                IsPopular = isPopular
            };

            var lots = await dbService.AuctionService.GetAllLots(filterParams);

            if(lots == null) 
                return Json(new { success = false, errorMessage = "Looks like there was an error loading the lots, or they ran out." });

            var lotsDto = lots.Select(lot => new {
                LotId = lot.LotId,
                Title = lot.Title,
                EndTime = lot.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                CurrentPrice = lot.CurrentPrice,
                Location = lot.Location,
                ImgPath = lot.ImgPath
            }).ToList();

            if(lotsDto == null || lotsDto.Count == 0)
                return Json(new { success = true, errorMessage = "There are no more lots!" });

            return Json(new { success = true, lots = lotsDto });
        }

        [Authorize]
        [HttpGet]
        public IActionResult SellLot()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SellLot(CreateLotView model, List<IFormFile>? additionalPhotos)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, errorMessage = errors });
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var uploadResults = new List<ImageUploadResult>();

            var uploadResultMain = await _cloudinaryService.UploadImageAsync(model.ImgPath, $"lots/user - {userId}");

            Lot lot = new Lot
            {
                Title = model.Title,
                ImgPath = uploadResultMain.SecureUrl.ToString(),
                Description = model.Description,
                StartPrice = model.StartPrice,
                EndTime = model.EndTime,
                Location = model.Location,
                CreatedAt = DateTime.Now,
                UserId = int.Parse(userId)
            };

            if(!await dbService.AuctionService.AddLot(lot))
            {
                return Json(new { success = false, errorMessage = "error when adding your lot to the database!" });
            }

            if(additionalPhotos != null && additionalPhotos.Count != 0)
            {
                foreach (var photo in additionalPhotos)
                {
                    if (photo.Length > 0)
                    {
                        // Загрузка фото в папку пользователя на Cloudinary
                        var uploadResult = await _cloudinaryService.UploadImageAsync(photo, $"lots/user - {userId}");
                        uploadResults.Add(uploadResult);
                    }
                }

                List<ImgLot> images = new List<ImgLot>();

                // Сохранение путей к загруженным фотографиям в базу данных
                foreach (var result in uploadResults)
                {
                    var imgPath = result.SecureUrl.ToString();
                    images.Add(new ImgLot { ImgPath = imgPath, LotId = lot.LotId });
                }

                if (!await dbService.AuctionService.AddImgLots(images))
                {
                    return Json(new { success = false, errorMessage = "error when adding your photos to the database!" });
                }
            }

            return Json(new { success = true });
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChangeLot(int? idLot)
        {
            int IdUser = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Lot lot = await dbService.AuctionService.GetLot(idLot);

            if (IdUser != lot.UserId) return View("~/Views/Home/ErrorMessage.cshtml", "You do not have permission to edit this lot!");

            return View(lot);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeLot(Lot? lot, List<FileUploadModel>? additionalPhotos, IFormFile? imgMainPath)
        {
            if (!ModelState.IsValid)
            {
                var missingFields = new List<string>();

                if (string.IsNullOrWhiteSpace(lot.Title))
                {
                    missingFields.Add("Title");
                }

                if (string.IsNullOrWhiteSpace(lot.Description))
                {
                    missingFields.Add("Description");
                }

                if (lot.StartPrice <= 0)
                {
                    missingFields.Add("Start Price");
                }

                if (lot.EndTime == default(DateTime))
                {
                    missingFields.Add("End Time");
                }

                if (string.IsNullOrWhiteSpace(lot.Location))
                {
                    missingFields.Add("Location");
                }

                if (missingFields.Any())
                {
                    string errorMessage = $"Please fill in the following fields: {string.Join(", ", missingFields)}.";
                    return Json(new { success = false, errorMessage });
                }

            }

            Lot uploadLot = await dbService.AuctionService.GetLot(lot.LotId);

            uploadLot.Title = lot.Title;
            uploadLot.Description = lot.Description;
            uploadLot.EndTime = lot.EndTime;
            uploadLot.StartPrice = lot.StartPrice;
            uploadLot.Location = lot.Location;

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));


            if(imgMainPath != null)
            {
                var uploadResultMain = await _cloudinaryService.UploadImageAsync(imgMainPath, $"lots/user - {userId}");
                uploadLot.ImgPath = uploadResultMain.SecureUrl.ToString();
            }

            if(!await dbService.AuctionService.UpdateLot(uploadLot)) 
                return Json(new { success = false, errorMessage = "Error when updating Lot!" });

            List<ImgLot> delImgLot = new List<ImgLot>();

            foreach(var it in uploadLot.ImgLots)
            {
                bool exists = additionalPhotos.Exists(i => i.Path == it.ImgPath);

                if (!exists)
                {
                    delImgLot.Add(it);

                    var deletionResult = await _cloudinaryService.DeleteImageAsync(it.ImgPath);

                    if (deletionResult.Result != "ok")
                    {
                        Console.WriteLine("Error when deleting image from cloudinary.");
                    }
                }
            }

            await dbService.AuctionService.DelImgLots(delImgLot);

            var uploadResults = new List<ImageUploadResult>();

            foreach (var it in additionalPhotos)
            {
                if(it.File != null)
                {
                    var uploadResult = await _cloudinaryService.UploadImageAsync(it.File, $"lots/user - {userId}");
                    uploadResults.Add(uploadResult);
                }
            }

            List<ImgLot> images = new List<ImgLot>();

            // Сохранение путей к загруженным фотографиям в базу данных
            foreach (var result in uploadResults)
            {
                var imgPath = result.SecureUrl.ToString();
                images.Add(new ImgLot { ImgPath = imgPath, LotId = uploadLot.LotId });
            }

            if(images.Count != 0 && !await dbService.AuctionService.AddImgLots(images))
                return Json(new { success = false, errorMessage = "error when adding your photos to the database!" });
             
            return Json(new { success = true, idLot = uploadLot.LotId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteLot(int? idLot)
        {
            int IdUser = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            Lot lot = await dbService.AuctionService.GetLot(idLot);

            if (IdUser != lot.UserId) return Json(new { success = false, errorMessage = "You do not have permission to edit this lot!" });

            if(lot.TotalBids != null) return Json(new { success = false, errorMessage = "Your auction already has bids, you cannot delete it!" });


            var deletionResults = await _cloudinaryService.DeleteImagesLotAsync(lot);

            foreach(var deletionResult in deletionResults)
            {
                if (deletionResult.Result != "ok")
                {
                    Console.WriteLine("Error when deleting image from cloudinary.");
                }
            }


            if (! await dbService.AuctionService.DeleteLot(idLot)) return Json(new { success = false, errorMessage = "Error when deleting from database!" });

            return Json(new { success = true });
        }

        [Authorize]
        [HttpPost]
        public async Task ConnectLotChat(int? idLot)
        {
            int IdUser = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _notification.ConnectionLotChat(idLot.Value, IdUser);
        }

        [Authorize]
        [HttpPost]
        public async Task DisconnectedLotChat(int? idLot)
        {
            int IdUser = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _notification.DisconnectedLotChat(idLot.Value, IdUser);
        }
    }
}
