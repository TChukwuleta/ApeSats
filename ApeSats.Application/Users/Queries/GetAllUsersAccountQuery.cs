using ApeSats.Application.Common.Interfaces;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApeSats.Application.Users.Queries
{
    public class GetAllUsersAccountQuery : IRequest<Result>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string UserId { get; set; }
    }

    public class GetAllUsersAccountQueryHandler : IRequestHandler<GetAllUsersAccountQuery, Result>
    {
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;
        public GetAllUsersAccountQueryHandler(IAppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<Result> Handle(GetAllUsersAccountQuery request, CancellationToken cancellationToken)
        {
            try
            {
                object response = default;
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to generate invoice. Invalid user specified");
                }
                var accounts = await _context.Accounts.ToListAsync();
                if (accounts == null || accounts.Count() <= 0)
                {
                    return Result.Failure("No accounts found");
                }
                if (request.Skip == 0 && request.Take == 0)
                {
                    response = new
                    {
                        Accounts = accounts,
                        Count = accounts.Count()
                    };
                }
                else
                {
                    response = new
                    {
                        Accounts = accounts.Skip(request.Skip).Take(request.Take).ToList(),
                        Count = accounts.Count()
                    };
                }
                return Result.Success("Accounts retrieval was successful", response);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Users account retrieval was not successful. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
