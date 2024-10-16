namespace Auction.Models
{
    public class LotFilterParams
    {
        public int? Size { get; set; }
        public int? IdLastLot { get; set; }
        public DateTime? DateTime { get; set; }
        public int? Price { get; set; }
        public string Location { get; set; }
        public bool? IsPopular { get; set; }
    }
}
