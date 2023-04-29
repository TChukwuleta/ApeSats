using ApeSats.Application.Common.Interfaces;
using ApeSats.Core.Entities;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ApeSats.Application.Arts.Commands
{
    public class RecordArtRequestCommand : IRequest<Result>
    {
    }

    public class RecordArtRequestCommandHandler : IRequestHandler<RecordArtRequestCommand, Result>
    {
        private readonly IAppDbContext _context;
        public RecordArtRequestCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(RecordArtRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var arts = await _context.Arts.Where(c => c.ArtStatus == Core.Enums.ArtStatus.Published).ToListAsync();
                if (arts == null || arts.Count() <= 0)
                {
                    return Result.Failure("No arts found");
                }
                foreach (var art in arts)
                {
                    var bid = await _context.Bids.FirstOrDefaultAsync(b => b.ArtNumber == art.Id && art.BidExpirationTime > DateTime.Now && b.Status == Core.Enums.Status.Active);
                    if (bid != null)
                    {
                        var artRequest = await _context.ArtRequests.FirstOrDefaultAsync(c => c.SellerAccount == bid.SellerAccountNumber
                        && c.BuyerAccount == bid.BuyerAccountNumber
                        && c.ArtId == bid.ArtNumber);
                        if (artRequest == null)
                        {
                            var newRequest = new ArtRequest
                            {
                                Settled = false,
                                BuyerAccount = bid.BuyerAccountNumber,
                                BidId = bid.Id,
                                SellerAccount = bid.SellerAccountNumber,
                                ArtId = bid.ArtNumber,
                                CreatedDate = DateTime.Now,
                                Status = Core.Enums.Status.Active
                            };
                            await _context.ArtRequests.AddAsync(newRequest);
                        }
                    }
                }
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success("Art request recording was successful");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Art request recording was not successful. {ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
