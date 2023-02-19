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

namespace ApeSats.Application.Accounts.Queries
{
    public class GetAccountByIdQuery : IRequest<Result>, IIdValidator
    {
        public int Id { get; set; }
        public string UserId { get; set; }
    }

    public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, Result>
    {
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;

        public GetAccountByIdQueryHandler(IAppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }
        public async Task<Result> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Account retrieval was not successful. Invalid user details");
                }
                var account = await _context.Accounts.FirstOrDefaultAsync(c => c.UserId == request.UserId && c.Id == request.Id);
                if (account == null)
                {
                    return Result.Failure("Account retrieval was not successful. Invalid account specified");
                }
                return Result.Success("Account retrieval was successful.", account);
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "Account retrieval was not successful", ex?.Message ?? ex?.InnerException.Message });
            }
        }
    }
}

