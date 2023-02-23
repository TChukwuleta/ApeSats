using ApeSats.Application.Arts.Commands;
using ApeSats.Application.Arts.Queries;
using ApeSats.Application.Transactions.Queries;
using ApeSats.Application.Users.Commands;
using ApeSats.Core.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApeSats.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtController : ApiController
    {
        protected readonly IHttpContextAccessor _contextAccessor;
        private readonly string accessToken;
        public ArtController(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            accessToken = _contextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("You are not authorized!");
            }
        }

        [HttpPost("uploadart")]
        public async Task<ActionResult<Result>> UploadArt(CreateArtCommand command)
        {
            try
            {
                return await Mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to upload art. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpPost("updateart")]
        public async Task<ActionResult<Result>> UpdateArt(UpdateArtCommand command)
        {
            try
            {
                return await Mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to art art. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpPost("publishart")]
        public async Task<ActionResult<Result>> PublishArt(PublishArtCommand command)
        {
            try
            {
                return await Mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to publish art. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpPost("bidforart")]
        public async Task<ActionResult<Result>> BidForArt(BidForArtCommand command)
        {
            try
            {
                return await Mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to bid for art. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpPost("generateinvoiceforart")]
        public async Task<ActionResult<Result>> GenerateInvoiceForArt(GenerateInvoiceForArtCommand command)
        {
            try
            {
                return await Mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to generate invoice for art. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpPost("invoicelistener")]
        public async Task<ActionResult<Result>> InvoiceListener(ListenForInvoiceCommand command)
        {
            try
            {
                return await Mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to listen for invoice. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpPost("withdrawsatoshis")]
        public async Task<ActionResult<Result>> WithdrawSatoshis(WithdrawSatoshiCommand command)
        {
            try
            {
                return await Mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed make payment. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }


        [HttpGet("gettransactionsbyid/{skip}/{take}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetAllTransactionsByUser(int skip, int take, string userid)
        {
            try
            {
                return await Mediator.Send(new GetAllTransactionsQuery { Skip = skip, Take = take, UserId = userid });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Transactions retrieval by user failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getallarts/{skip}/{take}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetAllArts(int skip, int take, string userid)
        {
            try
            {
                return await Mediator.Send(new GetAllArtsQuery { Skip = skip, Take = take, UserId = userid });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Arts retrieval failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getpublishedarts/{skip}/{take}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetPublishedArts(int skip, int take, string userid)
        {
            try
            {
                return await Mediator.Send(new GetAllPublishedArtsQuery { Skip = skip, Take = take, UserId = userid });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Published arts retrieval failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getartsbyuser/{skip}/{take}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetArtsByUser(int skip, int take, string userid)
        {
            try
            {
                return await Mediator.Send(new GetAllUserArtsQuery { Skip = skip, Take = take, UserId = userid });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Arts retrieval by user failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getbyid/{id}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetById(int id, string userid)
        {
            try
            {
                return await Mediator.Send(new GetArtByIdQuery { Id = id, UserId = userid });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Art retrieval by id failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }
    }
}
