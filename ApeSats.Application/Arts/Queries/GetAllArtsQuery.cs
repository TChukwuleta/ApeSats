using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Interfaces.Validators;
using ApeSats.Core.Entities;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApeSats.Application.Arts.Queries
{
    public class GetAllArtsQuery : IRequest<Result>, IBaseValidator
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string UserId { get; set; }
    }

    public class GetAllArtsQueryHandler : IRequestHandler<GetAllArtsQuery, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;
        public GetAllArtsQueryHandler(IAuthService authService, IAppDbContext context)
        {
            _authService = authService;
            _context = context;
        }
        public async Task<Result> Handle(GetAllArtsQuery request, CancellationToken cancellationToken)
        {
            var arts = new List<Art>();
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Arts retrieval was not successful. Invalid user details");
                }
                var allArts = await _context.Arts.Select(item => new Art
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    Image = item.Image,
                    Status = item.Status,
                    CreatedDate = item.CreatedDate
                }).ToListAsync();
                if (allArts == null || allArts.Count() <= 0)
                {
                    return Result.Failure("No arts available");
                }
                if (request.Skip == 0 && request.Take == 0)
                {
                    arts = allArts;
                }
                else
                {
                    arts = allArts.Skip(request.Skip).Take(request.Take).ToList();
                }

                return Result.Success("All arts retrieval was successful", arts);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Arts retrieval was not successful. {ex?.Message ?? ex?.InnerException.Message }");
            }
        }
    }
}
