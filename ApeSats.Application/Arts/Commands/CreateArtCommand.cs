using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Interfaces.Validators;
using ApeSats.Core.Entities;
using ApeSats.Core.Enums;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ApeSats.Application.Arts.Commands
{
    public class CreateArtCommand : IRequest<Result>, IBaseValidator
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal BaseAmount { get; set; }
        public string Image { get; set; }
        public string UserId { get; set; }
    }

    public class CreateArtCommandHandler : IRequestHandler<CreateArtCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IAppDbContext _context;
        private readonly IConfiguration _config;
        public CreateArtCommandHandler(IAuthService authService, ICloudinaryService cloudinaryService, IAppDbContext context, IConfiguration config)
        {
            _config = config;
            _authService = authService;
            _cloudinaryService = cloudinaryService;
            _context = context;
        }
        public async Task<Result> Handle(CreateArtCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Post creation was not successful. Invalid user details specified");
                }
                if (string.IsNullOrEmpty(request.Image))
                {
                    return Result.Failure("Kindly pass in the base64 string of the image intended for this post");
                }
                var existingPost = await _context.Arts.FirstOrDefaultAsync(c => c.Title.ToLower() == request.Title.ToLower());
                if (existingPost != null)
                {
                    return Result.Failure("Art upload was not successful. Another art with this title already exist");
                }
                var minAmount = _config["Art:MinimumAmount"];
                var maxAmount = _config["Art:MaximumAmount"];
                if (request.BaseAmount < int.Parse(minAmount))
                {
                    return Result.Failure("Amount specified less than the minimum specified amount by the system");
                }
                if (request.BaseAmount > int.Parse(maxAmount))
                {
                    return Result.Failure("Amount specified is greater than the maximum specified amount by the system");
                }
                var art = new Art
                {
                    Title = request.Title,
                    Image = string.IsNullOrEmpty(request.Image) ? "" : await _cloudinaryService.UploadImage(request.Image, request.UserId),//request.Image, // Do cloudinary for image
                    BaseAmount = request.BaseAmount,
                    CreatedDate = DateTime.Now,
                    ArtStatus = ArtStatus.Draft,
                    Status = Status.Active,
                    UserId = request.UserId,
                    Description = request.Description
                };
                await _context.Arts.AddAsync(art);
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success("Art upload creation was successful", art);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Art creation was not successful. {ex?.Message ?? ex?.InnerException.Message }");
            }
        }
    }
}
