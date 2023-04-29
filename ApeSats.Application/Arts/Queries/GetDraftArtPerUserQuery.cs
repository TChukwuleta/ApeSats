using ApeSats.Application.Common.Interfaces;
using ApeSats.Core.Entities;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApeSats.Application.Arts.Queries
{
    public class GetDraftArtPerUserQuery : IRequest<Result>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string UserId { get; set; }
    }

    public class GetDraftArtPerUserQueryHandler : IRequestHandler<GetDraftArtPerUserQuery, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;
        public GetDraftArtPerUserQueryHandler(IAuthService authService, IAppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        public async Task<Result> Handle(GetDraftArtPerUserQuery request, CancellationToken cancellationToken)
        {
            var arts = new List<Art>();
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Draft arts retrieval was not successful. Invalid user details specified");
                }
                var publishedArts = await _context.Arts.Where(c => c.UserId == request.UserId && c.ArtStatus == Core.Enums.ArtStatus.Draft).ToListAsync();
                if (publishedArts.Count() <= 0)
                {
                    return Result.Failure("Draft arts retrieval was not successful. No draft arts found for this user");
                }
                if (request.Skip == 0 && request.Take == 0)
                {
                    arts = publishedArts;
                }
                else
                {
                    arts = publishedArts.Skip(request.Skip).Take(request.Take).ToList();
                }
                var entity = new
                {
                    Entity = arts,
                    Count = publishedArts.Count()
                };
                return Result.Success("Draft arts retrieval was successful", arts);
            }
            catch (Exception ex)
            {
                return Result.Failure($"User draft arts retrieval was not successful. {ex?.Message ?? ex?.InnerException.Message }");
            }
        }
    }
}
