namespace ApeSats.Core.Entities
{
    public class ArtRequest : AuditableEntity
    {
        public string SellerAccount { get; set; }
        public string BuyerAccount { get; set; }
        public int BidId { get; set; }
        public int ArtId { get; set; }
        public bool Settled { get; set; }
    }
}
