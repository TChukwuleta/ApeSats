using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Interfaces.Validators;
using ApeSats.Core.Entities;
using ApeSats.Core.Enums;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApeSats.Application.Transactions.Commands
{
    public class CreateTransactionCommand : IRequest<Result>, ITransactionRequestValidator, IBaseValidator
    {
        public string Description { get; set; }
        public string DebitAccount { get; set; }
        public string CreditAccount { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string UserId { get; set; }
    }

    public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;

        public CreateTransactionCommandHandler(IAppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }
        public async Task<Result> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
        {
            var reference = $"BitPaywall_{DateTime.Now.Ticks}";
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Transaction creation failed. Invalid user details");
                }
                var account = await _context.Accounts.FirstOrDefaultAsync(c => c.UserId == request.UserId);
                if (account == null)
                {
                    return Result.Failure("Unable to create transaction. Account does not exist for this useer");
                }
                var entity = new Transaction
                {
                    DebitAccount = request.DebitAccount,
                    CreditAccount = request.CreditAccount,
                    UserId = request.UserId,
                    Amount = request.Amount,
                    TransactionType = request.TransactionType,
                    TransactionReference = reference,
                    TransactionStatus = TransactionStatus.Success,
                    Narration = request.Description,
                    CreatedDate = DateTime.Now,
                    AccountId = account.Id
                };
                switch (request.TransactionType)
                {
                    case TransactionType.Debit:
                        account.AvailableBalance -= request.Amount;
                        account.LedgerBalance -= request.Amount;
                        break;
                    case TransactionType.Credit:
                        account.AvailableBalance += request.Amount;
                        account.LedgerBalance += request.Amount;
                        break;
                    default:
                        break;
                }
                _context.Accounts.Update(account);
                await _context.Transactions.AddAsync(entity);
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success("Transaction creation was successful", entity);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Transactions creation was not successful. {ex?.Message ?? ex?.InnerException.Message }");
            }
        }
    }
}
