namespace Auction.Models.DBModels
{
    public class Lot
    {
        public int LotId { get; set; }              
        public string Title { get; set; }
        public string ImgPath { get; set; }
        public string? Description { get; set; }     
        public decimal StartPrice { get; set; }     
        public decimal? CurrentPrice { get; set; }
        public int? TotalBids { get; set; }
        public DateTime EndTime { get; set; }       
        public DateTime CreatedAt { get; set; } 
        public int? TotalViews { get; set; }
        public string Location { get; set; }

        public int UserId { get; set; }
        public int? UserIdBid { get; set; }

        public virtual User? User { get; set; }
        public virtual ICollection<Purchase>? Purchases { get; set; }
        public virtual ICollection<ImgLot>? ImgLots { get; set; }
    }
}
