using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Model.Request;
using ApeSats.Core.Entities;
using ApeSats.Core.Enums;
using ApeSats.Core.Model;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApeSats.Application.Transactions.Commands
{
    public class CreateTransactionsCommand : IRequest<Result>
    {
        public List<TransactionRequest> TransactionRequests { get; set; }
    }

    public class CreateTransactionsCommandHandler : IRequestHandler<CreateTransactionsCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;
        public CreateTransactionsCommandHandler(IAuthService authService, IAppDbContext context)
        {
            _authService = authService;
            _context = context;
        }
        public async Task<Result> Handle(CreateTransactionsCommand request, CancellationToken cancellationToken)
        {
            var reference = $"BitPaywall_{DateTime.Now.Ticks}";
            try
            {
                var transactions = new List<Transaction>();
                foreach (var item in request.TransactionRequests)
                {
                    var user = await _authService.GetUserById(item.UserId);
                    if (user.user == null)
                    {
                        return Result.Failure("Transaction creation failed. Invalid user details");
                    }
                    this.ValidateItem(item);
                    var userAccount = await _context.Accounts.FirstOrDefaultAsync(c => c.UserId == item.UserId);
                    var entity = new Transaction
                    {
                        UserId = item.UserId,
                        Amount = item.Amount,
                        CreditAccount = item.CreditAccount,
                        TransactionType = item.TransactionType,
                        TransactionReference = reference,
                        DebitAccount = item.DebitAccount,
                        TransactionStatus = TransactionStatus.Success,
                        AccountId = userAccount.Id,
                        CreatedDate = DateTime.Now,
                        Narration = item.Description,
                        Status = Status.Active
                    };
                    transactions.Add(entity);
                    switch (item.TransactionType)
                    {
                        case TransactionType.Debit:
                            userAccount.AvailableBalance -= item.Amount;
                            userAccount.LedgerBalance -= item.Amount;
                            break;
                        case TransactionType.Credit:
                            userAccount.AvailableBalance += item.Amount;
                            userAccount.LedgerBalance += item.Amount;
                            break;
                        default:
                            break;
                    }
                    _context.Accounts.Update(userAccount);
                }
                await _context.Transactions.AddRangeAsync(transactions);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success("Transactions created successfully", transactions);
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "Transactions creation was not successful", ex?.Message ?? ex?.InnerException.Message });
            }
        }

        private void ValidateItem(TransactionRequest item)
        {
            TransactionRequestValidator validator = new TransactionRequestValidator();
            ValidationResult validationResult = validator.Validate(item);
            string validateError = null;
            if (!validationResult.IsValid)
            {
                foreach (var failure in validationResult.Errors)
                {
                    validateError += "Property " + failure.PropertyName + " failed validation. Error was: " + failure.ErrorMessage + "\n";
                }
                throw new Exception(validateError);
            }
        }
    }
}
 