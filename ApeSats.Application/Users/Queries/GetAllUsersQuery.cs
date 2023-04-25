using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Interfaces.Validators;
using ApeSats.Core.Model;
using MediatR;

namespace ApeSats.Application.Users.Queries
{
    public class GetAllUsersQuery : IRequest<Result>, IEmailValidator
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string Email { get; set; }
    }

    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result>
    {
        private readonly IAuthService _authService;
        public GetAllUsersQueryHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserByEmail(request.Email);
                if (user.user == null)
                {
                    return Result.Failure("User rettrieval was not successful");
                }
                var result = await _authService.GetAllUsers(request.Skip, request.Take);
                if (result.users == null)
                {
                    return Result.Failure("No users found");
                }
                return Result.Success(result.users);
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "Getting all system users was not successful", ex?.Message ?? ex?.InnerException.Message });
            }
        }
    }
}
