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

namespace ApeSats.Application.Arts.Queries
{
    public class GetArtByIdQuery : IRequest<Result>, IIdValidator
    {
        public int Id { get; set; }
        public string UserId { get; set; }
    }

    public class GetArtByIdQueryHandler : IRequestHandler<GetArtByIdQuery, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;

        public GetArtByIdQueryHandler(IAuthService authService, IAppDbContext context)
        {
            _context = context;
            _authService = authService;
        }
        public async Task<Result> Handle(GetArtByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to retrieve art. Invalid user details");
                }
                var art = await _context.Arts.Include(c => c.Bid).FirstOrDefaultAsync(c => c.Id == request.Id);
                if (art == null)
                {
                    return Result.Failure("Unable to retrieve art. Invalid art details speicified");
                }
                return Result.Success("Art retrieval was successful", art);
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "User art retrieval was not successful", ex?.Message ?? ex?.InnerException.Message });
            }
        }
    }
}
