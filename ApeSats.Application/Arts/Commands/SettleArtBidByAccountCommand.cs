using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Model.Request;
using ApeSats.Application.Transactions.Commands;
using ApeSats.Core.Enums;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApeSats.Application.Arts.Commands
{
    public class SettleArtBidByAccountCommand : IRequest<Result>
    {
    }

    public class SettleArtBidByAccountCommandHandler : IRequestHandler<SettleArtBidByAccountCommand, Result>
    {
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;
        public SettleArtBidByAccountCommandHandler(IAppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<Result> Handle(SettleArtBidByAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var artRequests = await _context.ArtRequests.Where(c => c.Settled == false || c.Status != Core.Enums.Status.Active).ToListAsync();
                if (artRequests == null || artRequests.Count() <= 0)
                {
                    return Result.Failure("No arts found");
                }
                foreach (var artRequest in artRequests)
                {
                    if (artRequest.CreatedDate > DateTime.Now.AddDays(-1))
                    {
                        var art = await _context.Arts.FirstOrDefaultAsync(c => c.Id == artRequest.ArtId);
                        if (art == null)
                        {
                            continue;
                        }
                        var bid = await _context.Bids.FirstOrDefaultAsync(c => c.Id == artRequest.BidId && c.ArtNumber == art.Id);
                        if (bid == null)
                        {
                            continue;
                        }
                        var buyerAccount = await _context.Accounts.FirstOrDefaultAsync(c => c.AccountNumber == artRequest.BuyerAccount);
                        if (buyerAccount == null)
                        {
                            continue;
                        }
                        var sellerAccount = await _context.Accounts.FirstOrDefaultAsync(c => c.AccountNumber == artRequest.SellerAccount);
                        if (sellerAccount == null)
                        {
                            continue;
                        }
                        buyerAccount.LockedBalance -= bid.Amount;
                        buyerAccount.AvailableBalance += bid.Amount;
                        var transactionRequests = new List<TransactionRequest>();
                        transactionRequests.Add(new TransactionRequest
                        {
                            Description = "Art payed for successfully",
                            DebitAccount = buyerAccount.AccountNumber,
                            CreditAccount = sellerAccount.AccountNumber,
                            Amount = bid.Amount,
                            TransactionType = TransactionType.Debit,
                            UserId = buyerAccount.UserId
                        });

                        transactionRequests.Add(new TransactionRequest
                        {
                            Description = "Art payed for successfully",
                            DebitAccount = buyerAccount.AccountNumber,
                            CreditAccount = sellerAccount.AccountNumber,
                            Amount = bid.Amount,
                            TransactionType = TransactionType.Credit,
                            UserId = sellerAccount.UserId
                        });
                        var createTransactionsRequest = new CreateTransactionsCommand
                        {
                            TransactionRequests = transactionRequests
                        };
                        var createTransaction = await new CreateTransactionsCommandHandler(_authService, _context).Handle(createTransactionsRequest, cancellationToken);
                        if (!createTransaction.Succeeded)
                        {
                            throw new ArgumentException("An error while creating a transaction. Kindly contact support");
                        }
                        art.BidStartTime = new DateTime();
                        art.BidExpirationTime = new DateTime();
                        art.ArtStatus = ArtStatus.Draft;
                        art.BaseAmount = bid.Amount;
                        art.RebidCount = 0;
                        art.UserId = buyerAccount.UserId;

                        artRequest.Status = Status.Deactivated;
                        artRequest.Settled = true;
                        _context.ArtRequests.Update(artRequest);
                        _context.Arts.Update(art);
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success("Art account settlement was successful");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Art account settlement was not successful. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
