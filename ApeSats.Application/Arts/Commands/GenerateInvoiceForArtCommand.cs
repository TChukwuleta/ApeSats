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

namespace ApeSats.Application.Arts.Commands
{
    public class GenerateInvoiceForArtCommand : IRequest<Result>, IIdValidator
    {
        public int Id { get; set; }
        public string UserId { get; set; }
    }

    public class GenerateInvoiceForArtCommandHandler : IRequestHandler<GenerateInvoiceForArtCommand, Result>
    {
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILightningService _lightningService;

        public GenerateInvoiceForArtCommandHandler(IAppDbContext context, IAuthService authService, ILightningService lightningService)
        {
            _context = context;
            _lightningService = lightningService;
            _authService = authService;
        }

        public async Task<Result> Handle(GenerateInvoiceForArtCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to generate invoice. Invalid user specified");
                }

                var account = await _context.Accounts.FirstOrDefaultAsync(c => c.UserId == request.UserId);
                if (account == null)
                {
                    return Result.Failure("Unable to generate invoice for art. Invalid user account");
                }
                var art = await _context.Arts.Include(c => c.Bid).FirstOrDefaultAsync(c => c.Id == request.Id);
                if (art == null)
                {
                    return Result.Failure("Unable to generate invoice for art. Invalid art");
                }
                if (art.Bid == null)
                {
                    return Result.Failure("Unable to generate invoice for art. No available bid for this art");
                }
                var userWonBid = art.Bid.BuyerAccountNumber == account.AccountNumber;
                if (!userWonBid)
                {
                    return Result.Failure("Unable to generate invoice for art. Apologies, this bid is not available to you");
                }
                var walletBalance = await _lightningService.GetWalletBalance(Core.Enums.UserType.Admin);
                if (walletBalance <= art.Bid.Amount)
                {
                    return Result.Failure("Invalid wallet balance. Kindly contact support");
                }
                var invoice = await _lightningService.CreateInvoice((long)art.Bid.Amount, $"{art.Id}/{request.UserId}", Core.Enums.UserType.Admin);
                return Result.Success("Invoice generation was successful. Kindly proceed to make payment",invoice);
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "Invoice generation was not successful", ex?.Message ?? ex?.InnerException.Message });
            }
        }
    }
}
