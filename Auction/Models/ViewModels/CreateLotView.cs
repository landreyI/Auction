using Auction.Models.DBModels;
using System.ComponentModel.DataAnnotations;

namespace Auction.Models.ViewModels
{
    public class CreateLotView
    {
        [Required(ErrorMessage = "The Title field is required.")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "The Title must be between 2 and 20 characters.")]
        public string Title { get; set; }

        public IFormFile ImgPath { get; set; }
        public string Description { get; set; } // Оставляем опциональным

        [Required(ErrorMessage = "The Start Price field is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Start Price must be greater than 0.")]
        public decimal StartPrice { get; set; }

        [Required(ErrorMessage = "The End Time field is required.")]
        public DateTime EndTime { get; set; }

        [StringLength(20, MinimumLength = 2, ErrorMessage = "The Location must be between 2 and 20 characters.")]
        [Required(ErrorMessage = "The Location field is required.")]
        public string Location { get; set; }
    }
}
