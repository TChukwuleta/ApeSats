using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Model.Response;
using ApeSats.Application.Transactions.Commands;
using ApeSats.Core.Entities;
using ApeSats.Core.Enums;
using ApeSats.Core.Model;
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

                var sellerAccount = await _context.Accounts.FirstOrDefaultAsync(c => c.UserId == art.UserId);
                if (sellerAccount == null)
                {
                    throw new ArgumentException("Invalid seller account");
                }

                var transactionRequest = new CreateTransactionCommand
                {
                    Description = "Art payed for successfully",
                    DebitAccount = account.AccountNumber,
                    CreditAccount = sellerAccount.AccountNumber,
                    Amount = art.Bid.Amount,
                    TransactionType = TransactionType.Debit,
                    UserId = request.UserId
                };


                sellerAccount.LedgerBalance += art.Bid.Amount;
                sellerAccount.AvailableBalance += art.Bid.Amount;

                account.LockedBalance -= art.Bid.Amount;

                art.BidStartTime = new DateTime();
                art.BidExpirationTime = new DateTime();
                art.ArtStatus = ArtStatus.Draft;
                art.RebidCount = 0;
                art.Bid = null;
                art.UserId = request.UserId;
                _context.Accounts.Update(account);
                _context.Arts.Update(art);

                
                var createTransaction = await new CreateTransactionCommandHandler(_context, _authService).Handle(transactionRequest, new CancellationToken());
                if (!createTransaction.Succeeded)
                {
                    throw new ArgumentException("An error while creating a transaction. Kindly contact support");
                }
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
