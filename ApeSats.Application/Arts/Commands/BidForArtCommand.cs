using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Interfaces.Validators;
using ApeSats.Core.Entities;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
            var reference = $"ApeSats_{DateTime.Now.Ticks}";
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to bid for art. Invalid user details specified.");
                }
                var userAccount = await _context.Accounts.FirstOrDefaultAsync(c => c.UserId == request.UserId);
                if (userAccount == null)
                {
                    return Result.Failure("User account isn't available");
                }
                var art = await _context.Arts.FirstOrDefaultAsync(c => c.Id == request.Id);
                if (art == null)
                {
                    return Result.Failure("Unable to bid for art. Invalid art specified");
                }
                if (art.UserId == request.UserId)
                {
                    return Result.Failure("Cannot Bid for self owned art");
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
                var existingBids = await _context.Bids.Where(c => c.SellerAccountNumber == sellerAccount.AccountNumber && c.ArtNumber == art.Id).ToListAsync();
                if (existingBids == null || existingBids.Count() <= 0)
                {
                    if (request.Amount <= art.BaseAmount)
                    {
                        return Result.Failure("Please input a higer value to bid for this item");
                    }
                }
                else
                {
                    if (request.Amount <= existingBids.Max(c => c.Amount))
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
                existingBids.ForEach(c => c.Status = Core.Enums.Status.Deactivated);
                _context.Bids.UpdateRange(existingBids);
                var bid = new Bid
                {
                    SellerAccountNumber = sellerAccount.AccountNumber,
                    BuyerAccountNumber = userAccount.AccountNumber,
                    Amount = request.Amount,
                    Status = Core.Enums.Status.Active,
                    Reference = reference,
                    ArtNumber = art.Id,
                    CreatedDate = DateTime.Now
                };
                await _context.Bids.AddAsync(bid);
                _context.Accounts.Update(userAccount);
                var lastBidAmount = existingBids.Max(c => c.Amount);
                var lastBid = existingBids.FirstOrDefault(c => c.Amount <= lastBidAmount);
                if (lastBid != null)
                {
                    var lastBidder = await _context.Accounts.FirstOrDefaultAsync(c => c.AccountNumber == lastBid.BuyerAccountNumber);
                    if (lastBidder != null)
                    {
                        lastBidder.LockedBalance -= lastBid.Amount;
                        lastBidder.AvailableBalance += lastBid.Amount;
                        _context.Accounts.Update(lastBidder);
                    }
                }
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success("You have successfully bid for art");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Art bidding was not successful. {ex?.Message ?? ex?.InnerException.Message }");
            }
        }
    }
}
