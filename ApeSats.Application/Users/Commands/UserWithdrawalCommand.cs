using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Interfaces.Validators;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Application.Users.Commands
{
    public class UserWithdrawalCommand : IRequest<Result>, IBaseValidator
    {
        public string PaymentRequest { get; set; }
        public string UserId { get; set; }
    }

    public class UserWithdrawalCommandHandler : IRequestHandler<UserWithdrawalCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;
        private readonly ILightningService _lightningService;

        public UserWithdrawalCommandHandler(IAuthService authService, IAppDbContext context, ILightningService lightningService)
        {
            _authService = authService;
            _context = context;
            _lightningService = lightningService;
        }

        public async Task<Result> Handle(UserWithdrawalCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("User withdrawal failed. Invalid user specified");
                }
                var amount = await _lightningService.GetValueFromInvoice(request.PaymentRequest, Core.Enums.UserType.User);
                if (amount <= 0)
                {
                    return Result.Failure("User withdrawal failed. Unable to retrieve value from invoice.");
                }
                var account = await _context.Accounts.FirstOrDefaultAsync(c => c.UserId == request.UserId);
                if (account == null)
                {
                    return Result.Failure("User withdrawal failed. No account available for this user");
                }
                account.LedgerBalance -= amount;
                account.AvailableBalance -= amount;
                var sendLightningError = await _lightningService.SendLightning(request.PaymentRequest, Core.Enums.UserType.User);
                if (!string.IsNullOrEmpty(sendLightningError))
                {
                    return Result.Failure(sendLightningError);
                }
                _context.Accounts.Update(account);
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Failure("User withdrawal was successful");
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "User withdrawal was not successful", ex?.Message ?? ex?.InnerException.Message });
            }
        }
    }
}
