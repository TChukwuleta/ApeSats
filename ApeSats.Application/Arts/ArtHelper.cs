using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Model.Response;
using ApeSats.Core.Entities;
using ApeSats.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Application.Arts
{
    internal class ArtHelper
    {
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;

        public ArtHelper(IAppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<Art> ChangeArtOwner(InvoiceSettlementResponse request)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    throw new ArgumentException("Invalid user details");
                }

                var account = await _context.Accounts.FirstOrDefaultAsync(c => c.UserId == request.UserId);
                if (account == null)
                {
                    throw new ArgumentException("Invalid user account");
                }
                var art = await _context.Arts.Include(c => c.Bid).FirstOrDefaultAsync(c => c.Id == request.ArtId);
                if (art == null)
                {
                    throw new ArgumentException("Invalid art specified");
                }

                account.LockedBalance -= art.Bid.Amount;

                art.BidStartTime = new DateTime();
                art.BidExpirationTime = new DateTime();
                art.ArtStatus = ArtStatus.Draft;
                art.RebidCount = 0;
                art.Bid = null;
                art.UserId = request.UserId;
                _context.Accounts.Update(account);
                _context.Arts.Update(art);
                await _context.SaveChangesAsync(new CancellationToken());
                return art;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
