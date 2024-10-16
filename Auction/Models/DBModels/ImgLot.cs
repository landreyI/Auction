namespace Auction.Models.DBModels
{
    public class ImgLot
    {
        public int ImgLotId { get; set; }
        public string ImgPath { get; set; }
        public int LotId { get; set; }
        public virtual Lot Lot { get; set; }
    }
}
