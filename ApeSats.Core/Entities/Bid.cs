using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Core.Entities
{
    public class Bid : AuditableEntity
    {
        public string SellerAccountNumber { get; set; }
        public string BuyerAccountNumber { get; set; }
        public int ArtNumber { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
