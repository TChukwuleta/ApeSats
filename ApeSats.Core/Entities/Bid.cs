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
        public decimal Amount { get; set; }
    }
}
