using Auction.Models.DBModels;

namespace Auction.Service
{
    public class AutoBidStorage
    {
        private readonly List<AutoBid> autoBids = new List<AutoBid>();

        public bool AddAutoBid(int idLot, int idUser, int maxBid, int rateBid)
        {
            try
            {
                autoBids.RemoveAll(auto => auto.idLot == idLot && auto.idUser == idUser);


                autoBids.Add(new AutoBid
                {
                    idLot = idLot,
                    idUser = idUser,
                    maxBid = maxBid,
                    rateBid = rateBid
                });

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public void RemoveAutoBid(AutoBid autoBid)
        {
            autoBids.Remove(autoBid);
        }

        public void RemoveAutoBid(int idLot, int idUser)
        {
            autoBids.RemoveAll(auto => auto.idLot == idLot && auto.idUser == idUser);
        }

        public void RemoveAutoBidsToLot(int idLot)
        {
            autoBids.RemoveAll(auto => auto.idLot == idLot);
        }

        public AutoBid? GetAutoBid(int idLot, int idUser)
        {
            return autoBids.FirstOrDefault(auto => auto.idLot == idLot && auto.idUser == idUser);
        }

        public List<AutoBid> GetAutoBidsToUser(int idUser)
        {
            return autoBids.Where(auto => auto.idUser == idUser).ToList();
        }

        public List<AutoBid> GetAutoBids() {  return autoBids; }
    }

    public class AutoBid
    {
        public int idLot { get; set; }
        public int idUser { get; set; }
        public int maxBid { get; set; }
        public int rateBid { get; set; }
    }
}
