using System.ComponentModel.DataAnnotations;

namespace Auction.Models.ViewModels
{
    public class CreateUserView
    {
        [Required(ErrorMessage = "Fill in all the fields")]
        [MinLength(3, ErrorMessage = "The nick-name must be at least 3 letters long")]
        [MaxLength(20, ErrorMessage = "The nick-name must be no more than 20 letters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Fill in all the fields")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Fill in all the fields")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "The password must be at least 6 characters long")]
        [MaxLength(30, ErrorMessage = "The password must be no more than 30 characters")]
        public string Password { get; set; }
    }
}
