using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Interfaces.Validators;
using ApeSats.Core.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Application.Arts.Commands
{
    public class ListenForInvoiceCommand : IRequest<Result>
    {
    }

    public class ListenForInvoiceCommandHandler : IRequestHandler<ListenForInvoiceCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly ILightningService _lightningService;
        private readonly IAppDbContext _context;
        public ListenForInvoiceCommandHandler(IAuthService authService, ILightningService lightningService, IAppDbContext context)
        {
            _authService = authService;
            _lightningService = lightningService;
            _context = context;
        }

        public async Task<Result> Handle(ListenForInvoiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var listener = await _lightningService.ListenForSettledInvoice(Core.Enums.UserType.Admin);
                if (listener == null)
                {
                    return Result.Failure("An error occured.");
                }
                var handler = await new ArtHelper(_context, _authService).ChangeArtOwner(listener);
                if (handler == null)
                {
                    return Result.Failure("An error occured.");
                }
                return Result.Success("Invoice has been confirmed and art is now yours",listener);
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "Invoice confirmation was not successful", ex?.Message ?? ex?.InnerException.Message });
            }
        }
    }
}
