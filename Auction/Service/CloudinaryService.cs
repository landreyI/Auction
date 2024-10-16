using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;
using Auction.Models.DBModels;

namespace Auction.Service
{
    public class CloudinarySettings
    {
        public string CloudName { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }

    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file, string folderPath)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderPath // Загрузка в указанную папку
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult;
        }
        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image // Тип ресурса - изображение
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result;
        }

        public async Task<List<DeletionResult>> DeleteImagesLotAsync(Lot? lot)
        {
            if (lot == null)
            {
                throw new ArgumentException("Lot is null.");
            }

            var imagePublicIds = new List<string>();

            if (!string.IsNullOrEmpty(lot.ImgPath))
            {
                imagePublicIds.Add(lot.ImgPath);
            }

            if (lot.ImgLots != null && lot.ImgLots.Count > 0)
            {
                imagePublicIds.AddRange(lot.ImgLots.Select(i => i.ImgPath));
            }

            var deletionResults = new List<DeletionResult>();

            foreach (var publicId in imagePublicIds)
            {
                var result = await DeleteImageAsync(publicId);
                deletionResults.Add(result);
            }

            return deletionResults;
        }
    }

}
