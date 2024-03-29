﻿using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Interfaces.Validators;
using ApeSats.Application.Transactions.Commands;
using ApeSats.Core.Entities;
using ApeSats.Core.Enums;
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
    public class WithdrawSatoshiCommand : IRequest<Result>, IBaseValidator
    {
        public string PaymentRequest { get; set; }
        public string UserId { get; set; }
    }

    public class WithdrawSatoshiCommandHandler : IRequestHandler<WithdrawSatoshiCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly ILightningService _lightningService;
        private readonly IAppDbContext _context;

        public WithdrawSatoshiCommandHandler(IAuthService authService, ILightningService lightningService, IAppDbContext context)
        {
            _authService = authService;
            _context = context;
            _lightningService = lightningService;
        }

        public async Task<Result> Handle(WithdrawSatoshiCommand request, CancellationToken cancellationToken)
        {
            var reference = $"ApeSats_{DateTime.Now.Ticks}";
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to withdraw satoshis. Invalid user details");
                }

                var account = await _context.Accounts.FirstOrDefaultAsync(c => c.UserId == request.UserId);
                if (account == null)
                {
                    return Result.Failure("Unable to withdraw satoshis. Invalid user account details");
                }
                var decodedInvoiceAmount = await _lightningService.DecodePaymentRequest(request.PaymentRequest);
                if (decodedInvoiceAmount <= 0)
                {
                    return Result.Failure("An error occured while trying to decode payment request. Please try again later");
                }
                if (account.AvailableBalance < decodedInvoiceAmount)
                {
                    return Result.Failure("Unable to withdraw satoshis. Insufficient funds in the account");
                }
                var payInvoiceResponse = await _lightningService.SendLightning(request.PaymentRequest);
                if (!string.IsNullOrEmpty(payInvoiceResponse))
                {
                    return Result.Failure($"An error occured while trying to pay invoice. {payInvoiceResponse}");
                }
                var transactionRequest = new CreateTransactionCommand
                {
                    Description = "Money withdrawal successfully",
                    DebitAccount = account.AccountNumber,
                    CreditAccount = "",
                    Amount = decodedInvoiceAmount,
                    TransactionType = TransactionType.Debit,
                    UserId = account.UserId
                };

                var fundingTransaction = await new CreateTransactionCommandHandler(_context, _authService).Handle(transactionRequest, cancellationToken);
                if (!fundingTransaction.Succeeded)
                {
                    return Result.Failure("An error while creating a transaction. Kindly contact support");
                }

                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success("Invoice has been paid successfully");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Invoice payment was not successful. {ex?.Message ?? ex?.InnerException.Message }");
            }
        }
    }
}
