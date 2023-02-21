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

namespace ApeSats.Application.Arts.Commands
{
    public class PublishArtCommand : IRequest<Result>, IIdValidator
    {
        public int Id { get; set; }
        public DateTime BidEndTime { get; set; }
        public string UserId { get; set; }
    }

    public class PublishArtCommandHandler : IRequestHandler<PublishArtCommand, Result>
    {
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;

        public PublishArtCommandHandler(IAppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }
        public async Task<Result> Handle(PublishArtCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to publish art. Invalid user details specified.");
                }
                var art = await _context.Arts.FirstOrDefaultAsync(c => c.UserId == request.UserId && c.Id == request.Id);
                if (art == null)
                {
                    return Result.Failure("Unable to publish art. Invalid art specified");
                }
                if (art.BidStartTime < DateTime.Now && art.BidExpirationTime > DateTime.Now)
                {
                    return Result.Failure("Unable to publish art. Art currently on bid");
                }
                if (request.BidEndTime < DateTime.Now)
                {
                    return Result.Failure("You cannot select a day earlier than now for your expiration");
                }
                if ((art.ArtStatus == Core.Enums.ArtStatus.Published || art.ArtStatus == Core.Enums.ArtStatus.Rebid) && art.BidExpirationTime < DateTime.Now)
                {
                    if (art.ArtStatus == Core.Enums.ArtStatus.Rebid)
                    {
                        art.RebidCount += 1;
                    }
                    art.ArtStatus = Core.Enums.ArtStatus.Rebid;
                }
                else
                {
                    art.ArtStatus = Core.Enums.ArtStatus.Published;
                }
                
                art.BidStartTime = DateTime.Now;
                art.BidExpirationTime = request.BidEndTime;
                _context.Arts.Update(art);
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success("Art published successfully", art);
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "Publishing art was not successful", ex?.Message ?? ex?.InnerException.Message });
            }
        }
    }
}
