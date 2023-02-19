using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Interfaces.Validators;
using ApeSats.Core.Entities;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Application.Arts.Queries
{
    public class GetAllPublishedArtsQuery : IRequest<Result>, IBaseValidator
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string UserId { get; set; }
    }

    public class GetAllPublishedArtsQueryHandler : IRequestHandler<GetAllPublishedArtsQuery, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;
        public GetAllPublishedArtsQueryHandler(IAppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }
        public async Task<Result> Handle(GetAllPublishedArtsQuery request, CancellationToken cancellationToken)
        {
            var arts = new List<Art>();
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Published arts retrieval was not successful. Invalid user details specified");
                }
                var publishedArts = await _context.Arts.Where(c => c.UserId == request.UserId && c.ArtStatus == Core.Enums.ArtStatus.Published).ToListAsync();
                if (publishedArts.Count() <= 0)
                {
                    return Result.Failure("Published arts retrieval was not successful. No published arts found for this user");
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
                return Result.Success("Published arts retrieval was successful", arts);
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "Published arts retrieval was not successful", ex?.Message ?? ex?.InnerException.Message });
            }
        }
    }
}
