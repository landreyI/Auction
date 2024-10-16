namespace Auction.Models.DBModels
{
    public class Purchase
    {
        public int PurchaseId { get; set; }       
        public DateTime PurchaseDate { get; set; }
        public decimal FinalPrice { get; set; }   

        public int LotId { get; set; }            
        public virtual Lot Lot { get; set; }      

        public int BuyerId { get; set; }          
        public virtual User Buyer { get; set; }
    }
}
