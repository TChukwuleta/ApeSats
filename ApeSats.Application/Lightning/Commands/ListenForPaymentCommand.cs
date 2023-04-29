using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Model.Request;
using ApeSats.Application.Transactions.Commands;
using ApeSats.Core.Enums;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApeSats.Application.Lightning.Commands
{
    public class ListenForPaymentCommand : IRequest<Result>
    {
    } 

    public class ListenForPaymentCommandHandler : IRequestHandler<ListenForPaymentCommand, Result>
    {
        private readonly ILightningService _lightningService;
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;
        public ListenForPaymentCommandHandler(ILightningService lightningService, IAppDbContext context, IAuthService authService)
        {
            _lightningService = lightningService;
            _context = context;
            _authService = authService;
        }

        public async Task<Result> Handle(ListenForPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var invoiceListener = await _lightningService.ListenForSettledInvoice();
                var fundingTypeResponse = invoiceListener.Type.Split('|');
                var fundingType = fundingTypeResponse[0];
                var fundingId = fundingTypeResponse[1];
                Enum.TryParse(fundingType, out PaymentType type);
                int.TryParse(fundingId, out int id);
                switch (type)
                {
                    case PaymentType.Funding:
                        var account = await _context.Accounts.FirstOrDefaultAsync(c => c.Id == id);
                        if (account != null)
                        {
                            var transactionRequest = new CreateTransactionCommand
                            {
                                Description = "Account funding successfully",
                                DebitAccount = "",
                                CreditAccount = account.AccountNumber,
                                Amount = invoiceListener.AmountInSat,
                                TransactionType = TransactionType.Credit,
                                UserId = account.UserId
                            };

                            var fundingTransaction = await new CreateTransactionCommandHandler(_context, _authService).Handle(transactionRequest, cancellationToken);
                            if (!fundingTransaction.Succeeded)
                            {
                                return Result.Failure("An error while creating a transaction. Kindly contact support");
                            }
                        }
                        break;
                    case PaymentType.Purchase:
                        var user = await _authService.GetUserById(invoiceListener.UserId);
                        if (user.user == null)
                        {
                            break;
                        }

                        var buyerAccount = await _context.Accounts.FirstOrDefaultAsync(c => c.UserId == invoiceListener.UserId);
                        if (buyerAccount == null)
                        {
                            break;
                        }
                        var art = await _context.Arts.FirstOrDefaultAsync(c => c.Id == id);
                        if (art == null)
                        {
                            break;
                        }

                        var sellerAccount = await _context.Accounts.FirstOrDefaultAsync(c => c.UserId == art.UserId);
                        if (sellerAccount == null)
                        {
                            break;
                        }
                        buyerAccount.LockedBalance -= invoiceListener.AmountInSat;
                        buyerAccount.AvailableBalance += invoiceListener.AmountInSat;
                        var transactionRequests = new List<TransactionRequest>();
                        transactionRequests.Add(new TransactionRequest
                        {
                            Description = "Art payed for successfully",
                            DebitAccount = buyerAccount.AccountNumber,
                            CreditAccount = sellerAccount.AccountNumber,
                            Amount = invoiceListener.AmountInSat,
                            TransactionType = TransactionType.Debit,
                            UserId = buyerAccount.UserId
                        });

                        transactionRequests.Add(new TransactionRequest
                        {
                            Description = "Art payed for successfully",
                            DebitAccount = buyerAccount.AccountNumber,
                            CreditAccount = sellerAccount.AccountNumber,
                            Amount = invoiceListener.AmountInSat,
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
                        art.BaseAmount = invoiceListener.AmountInSat;
                        art.RebidCount = 0;
                        art.UserId = buyerAccount.UserId;

                        var artRequest = await _context.ArtRequests.FirstOrDefaultAsync(c => c.ArtId == art.Id
                        && c.BuyerAccount == buyerAccount.AccountNumber
                        && c.SellerAccount == sellerAccount.AccountNumber);
                        if (artRequest != null)
                        {
                            artRequest.Status = Status.Deactivated;
                            artRequest.Settled = true;
                            _context.ArtRequests.Update(artRequest);
                        }
                        _context.Arts.Update(art);
                        await _context.SaveChangesAsync(cancellationToken);
                        break;
                    default:
                        break;
                }
                
                return Result.Success("Invoice payment completed successfully");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
