using System.ComponentModel.DataAnnotations;

namespace Auction.Models.DBModels
{
    public class Chat
    {
        [Key]
        public int IdChat { get; set; }
        public int IdUser1 { get; set; }
        public int IdUser2 { get; set; }

        public User User1 { get; set; }
        public User User2 { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
