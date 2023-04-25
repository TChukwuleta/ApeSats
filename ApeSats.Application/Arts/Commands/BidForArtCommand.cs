using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Interfaces.Validators;
using ApeSats.Core.Entities;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Application.Arts.Commands
{
    public class BidForArtCommand : IRequest<Result>, IIdValidator
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string UserId { get; set; }
    }

    public class BidForArtCommandHandler : IRequestHandler<BidForArtCommand, Result>
    {
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;
        public BidForArtCommandHandler(IAppDbContext context, IAuthService authService)
        {
            _authService = authService;
            _context = context;
        }
        public async Task<Result> Handle(BidForArtCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to publish art. Invalid user details specified.");
                }
                var userAccount = await _context.Accounts.FirstOrDefaultAsync(c => c.UserId == request.UserId);
                if (userAccount == null)
                {
                    return Result.Failure("User account isn't available");
                }
                var art = await _context.Arts.Include(c => c.Bid).FirstOrDefaultAsync(c => c.Id == request.Id);
                if (art == null)
                {
                    return Result.Failure("Unable to bid for art. Invalid art specified");
                }
                var compareDate = DateTime.Compare((DateTime)art.BidExpirationTime, DateTime.Now);
                if (compareDate < 0)
                {
                    return Result.Failure("Unable to bid for art. Bid already expired.");
                }
                var sellerAccount = await _context.Accounts.FirstOrDefaultAsync(c => c.UserId == art.UserId);
                if (sellerAccount == null)
                {
                    return Result.Failure("Invalid account details available for seller");
                }
                if (art.Bid == null)
                {
                    if (request.Amount <= art.BaseAmount)
                    {
                        return Result.Failure("Please input a higer value to bid for this item");
                    }
                }
                else
                {
                    if (request.Amount <= art.Bid.Amount)
                    {
                        return Result.Failure("Please input a higer value to bid for this item");
                    }
                }
                
                if (userAccount.AvailableBalance < request.Amount)
                {
                    return Result.Failure("Insufficient funds to make a bid");
                }
                userAccount.AvailableBalance -= request.Amount;
                userAccount.LockedBalance += request.Amount;

                var bid = new Bid
                {
                    SellerAccountNumber = sellerAccount.AccountNumber,
                    BuyerAccountNumber = userAccount.AccountNumber,
                    Amount = request.Amount,
                    Status = Core.Enums.Status.Active,
                    CreatedDate = DateTime.Now
                };
                art.Bid = bid;
                art.LastModifiedDate = DateTime.Now;
                _context.Accounts.Update(userAccount);
                _context.Arts.Update(art);
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success("You have successfully bid for art");
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "Art bidding was not successful", ex?.Message ?? ex?.InnerException.Message });
            }
        }
    }
}
