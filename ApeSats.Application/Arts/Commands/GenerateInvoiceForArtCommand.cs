using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Interfaces.Validators;
using ApeSats.Core.Enums;
using ApeSats.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;

namespace ApeSats.Application.Arts.Commands
{
    public class GenerateInvoiceForArtCommand : IRequest<Result>, IIdValidator
    {
        public int Id { get; set; }
        public string UserId { get; set; }
    }

    public class GenerateInvoiceForArtCommandHandler : IRequestHandler<GenerateInvoiceForArtCommand, Result>
    {
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILightningService _lightningService;
        private readonly ICloudinaryService _cloudinaryService;

        public GenerateInvoiceForArtCommandHandler(IAppDbContext context, IAuthService authService, ILightningService lightningService, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _lightningService = lightningService;
            _authService = authService;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<Result> Handle(GenerateInvoiceForArtCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to generate invoice. Invalid user specified");
                }

                var account = await _context.Accounts.FirstOrDefaultAsync(c => c.UserId == request.UserId);
                if (account == null)
                {
                    return Result.Failure("Unable to generate invoice for art. Invalid user account");
                }
                var art = await _context.Arts.FirstOrDefaultAsync(c => c.Id == request.Id && c.ArtStatus == ArtStatus.Published);
                if (art == null)
                {
                    return Result.Failure("Unable to generate invoice for art. Invalid art");
                }
                var bid = await _context.Bids.FirstOrDefaultAsync(c => c.ArtNumber == request.Id 
                && c.BuyerAccountNumber == account.AccountNumber);
                if (bid == null)
                {
                    return Result.Failure("Unable to generate invoice for art. Your account does not have a record for this bid");
                }
                if (bid.Status != Status.Active)
                {
                    return Result.Failure("There is a new bidder on this item. If you are still interested in theis art, kindly bid higher");
                }
                if (art.BidExpirationTime > DateTime.Now)
                {
                    return Result.Failure("Art bidding is still ongoing. Please try again");
                }
                var walletBalance = await _lightningService.GetWalletBalance();
                if (walletBalance <= bid.Amount)
                {
                    return Result.Failure("Invalid wallet balance. Kindly contact support");
                }
                var invoice = await _lightningService.CreateInvoice((long)bid.Amount, $"{(int)PaymentType.Purchase}|{art.Id}/{request.UserId}");
                if (string.IsNullOrEmpty(invoice))
                {
                    return Result.Failure("Cannot generate invoice at the moment. Please try again later");
                }

                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(invoice, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(10);
                qrCodeImage.Save($"{art.Id}_{request.UserId}.jpg");
                /*var imageBytes = BitmapToBytes(qrCodeImage);
                string SigBase64 = Convert.ToBase64String(imageBytes);*/


                var fileUrl = await _cloudinaryService.UploadInvoiceQRCode(art.Id, request.UserId);
                var response = new
                {
                    QRCode = fileUrl,
                    Invoice = invoice
                };

                return Result.Success("Invoice generation was successful. Kindly proceed to make payment",response);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Invoice generation was not successful. {ex?.Message ?? ex?.InnerException.Message }");
            }
        }


        private static Byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}
