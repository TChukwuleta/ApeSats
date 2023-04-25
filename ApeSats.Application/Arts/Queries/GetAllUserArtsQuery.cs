using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Interfaces.Validators;
using ApeSats.Core.Entities;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApeSats.Application.Arts.Queries
{
    public class GetAllUserArtsQuery : IRequest<Result>, IBaseValidator
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string UserId { get; set; }
    }

    public class GetAllUserArtsQueryHandler : IRequestHandler<GetAllUserArtsQuery, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;
        public GetAllUserArtsQueryHandler(IAuthService authService, IAppDbContext context)
        {
            _context = context;
            _authService = authService;
        }
        public async Task<Result> Handle(GetAllUserArtsQuery request, CancellationToken cancellationToken)
        {
            var response = new List<Art>();
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Retrieving all user's art was not successful. Invalid user details");
                }
                var arts = await _context.Arts.Where(c => c.UserId == request.UserId).ToListAsync();
                if (arts == null || arts.Count() <= 0)
                {
                    return Result.Failure("Retrieving user's arts was not successful. No art found for this user");
                }
                if (request.Skip == 0 && request.Take == 0)
                {
                    response = arts;
                }
                else
                {
                    response = arts.Skip(request.Skip).Take(request.Take).ToList();
                }
                var entity = new
                {
                    Entity = response,
                    Count = arts.Count()
                };
                return Result.Success("Retrieving user's art was successful", entity);
            }
            catch (Exception ex)
            {
                return Result.Failure($"User's art retrieval was not successful. {ex?.Message ?? ex?.InnerException.Message }");
            }
        }
    }
}
