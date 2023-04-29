using ApeSats.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<Art> Arts { get; set; }
        DbSet<Bid> Bids { get; set; }
        DbSet<Account> Accounts { get; set; }
        DbSet<ArtRequest> ArtRequests { get; set; }
        DbSet<Transaction> Transactions { get; set; }
        DbSet<SystemUser> SystemUsers { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
