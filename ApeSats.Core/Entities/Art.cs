using ApeSats.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Core.Entities
{
    public class Art : AuditableEntity
    {
        public string Image { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? BidExpirationTime { get; set; }
        public DateTime? BidStartTime { get; set; }
        public ArtStatus ArtStatus { get; set; }
        public string ArtStatusDesc { get { return ArtStatus.ToString(); } }
        public int RebidCount { get; set; }
        public Bid? Bid { get; set; }
        public decimal BaseAmount { get; set; }
        public string UserId { get; set; }
    }
}
