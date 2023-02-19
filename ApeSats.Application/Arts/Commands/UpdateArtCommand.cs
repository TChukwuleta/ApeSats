using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Interfaces.Validators;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Application.Arts.Commands
{
    public class UpdateArtCommand : IRequest<Result>, IIdValidator
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public decimal Amount { get; set; }
        public int Id { get; set; }
        public string UserId { get; set; }
    }

    public class UpdateArtCommandHandler : IRequestHandler<UpdateArtCommand, Result>
    {
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;
        private readonly ICloudinaryService _cloudinaryService;

        public UpdateArtCommandHandler(IAppDbContext context, IAuthService authService, IConfiguration config, ICloudinaryService cloudinaryService)
        {
            _authService = authService;
            _cloudinaryService = cloudinaryService;
            _config = config;
            _context = context;
        }
        public async Task<Result> Handle(UpdateArtCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Art update was not successful. Invalid user details");
                }
                var art = await _context.Arts.FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == request.UserId);
                if (art == null)
                {
                    return Result.Failure("Art update was not successful. Invalid art details");
                }
                if (art.ArtStatus == Core.Enums.ArtStatus.Published)
                {
                    return Result.Failure("Art update was not successful. Art already published");
                }
                var minAmount = _config["Art:MinimumAmount"];
                var maxAmount = _config["Art:MaximumAmount"];
                if (request.Amount < int.Parse(minAmount))
                {
                    return Result.Failure("Amount specified less than the minimum specified amount by the system");
                }
                if (request.Amount > int.Parse(maxAmount))
                {
                    return Result.Failure("Amount specified is greater than the maximum specified amount by the system");
                }
                art.Image = string.IsNullOrEmpty(request.Image) ? art.Image : await _cloudinaryService.UploadImage(request.Image, request.UserId); // cloudinary image
                art.Description = request.Description;
                art.Title = request.Title;
                art.BaseAmount = request.Amount;
                art.LastModifiedDate = DateTime.Now;
                _context.Arts.Update(art);
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success("Art update was successful", art);
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "Art update was not successful", ex?.Message ?? ex?.InnerException.Message });
            }
        }
    }
}
