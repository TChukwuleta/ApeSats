using ApeSats.Application.Common.Interfaces.Validators;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Application.Transactions
{
    public class TransactionValidator
    {
    }

    public class TransactionRequestValidator : AbstractValidator<ITransactionRequestValidator>
    {
        public TransactionRequestValidator()
        {
            RuleFor(c => c.Amount).NotEmpty().WithMessage("Amount must be specified");
            RuleFor(c => c.TransactionType).NotEmpty().WithMessage("Transaction must be specified");
            RuleFor(c => c.DebitAccount).NotEmpty().WithMessage("Debit account number must be specified");
            RuleFor(c => c.CreditAccount).NotEmpty().WithMessage("Credit account number must be specified");
        }
    }
}
