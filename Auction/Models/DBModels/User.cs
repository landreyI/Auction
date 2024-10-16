namespace Auction.Models.DBModels
{
    public class User
    {
        public int UserId { get; set; }         
        public string UserName { get; set; }    
        public string Email { get; set; }       
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Lot> Lots { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; }

        public ICollection<Chat> Chats1 { get; set; }
        public ICollection<Chat> Chats2 { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
