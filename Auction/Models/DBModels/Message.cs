using System.ComponentModel.DataAnnotations;

namespace Auction.Models.DBModels
{
    public class Message
    {
        [Key]
        public int IdMessage { get; set; }
        public int IdChat { get; set; }
        public int IdSender { get; set; }
        public string MessageText { get; set; }
        public DateTime SentDateTime { get; set; }

        public Chat Chat { get; set; }
        public User Sender { get; set; }
    }
}
