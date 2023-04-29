using ApeSats.Application.Transactions.Commands;
using ApeSats.Application.Transactions.Queries;
using ApeSats.Application.Users.Commands;
using ApeSats.Core.Model;
using ApeSats.Infrastructure.Utility;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApeSats.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TransactionController : ApiController
    {
        protected readonly IHttpContextAccessor _contextAccessor;
        private readonly IMediator _mediator;
        public TransactionController(IHttpContextAccessor contextAccessor, IMediator mediator)
        {
            _contextAccessor = contextAccessor;
            _mediator = mediator;
            accessToken = _contextAccessor.HttpContext.Request.Headers["Authorization"].ToString()?.ExtractToken();
            if (accessToken == null)
            {
                throw new Exception("You are not authorized!");
            }

        }

        [HttpGet("gettransactionsbyid/{skip}/{take}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetAllTransactionsByUser(int skip, int take, string userid)
        {
            try
            {
                return await _mediator.Send(new GetAllTransactionsQuery { Skip = skip, Take = take, UserId = userid });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Transactions retrieval by user failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getbyid/{id}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetTransactionById(int id, string userid)
        {
            try
            {
                return await _mediator.Send(new GetTransactionByIdQuery { Id = id, UserId = userid });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Transaction retrieval by id failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getallcredit/{skip}/{take}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetAllDebitTransactionsByUser(int skip, int take, string userid)
        {
            try
            {
                return await _mediator.Send(new GetCreditTransactionsByUserIdQuery { UserId = userid, Skip = skip, Take = take });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Credit transactions retrieval failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getcredittransaction/{skip}/{take}/{accountnumber}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetCreditTransactionsByAccountNumber(int skip, int take, string accountnumber, string userid)
        {
            try
            {
                accessToken.ValidateToken(userid);
                return await _mediator.Send(new GetCreditTransactionByAccountNumberQuery { AccountNumber = accountnumber, UserId = userid, Skip = skip, Take = take });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Credit transactions retrieval failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getalldebit/{skip}/{take}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetAllCreditTransactionsByUser(int skip, int take, string userid)
        {
            try
            {
                accessToken.ValidateToken(userid);
                return await _mediator.Send(new GetDebitTransactionByUserIdQuery { UserId = userid, Skip = skip, Take = take });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Debit transactions retrieval failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getdebittransaction/{skip}/{take}/{accountnumber}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetDebitTransactionsByAccountNumber(int skip, int take, string accountnumber, string userid)
        {
            try
            {
                accessToken.ValidateToken(userid);
                return await _mediator.Send(new GetDebitTransactionByAccountNumberQuery { AccountNumber = accountnumber, UserId = userid, Skip = skip, Take = take });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Debit transactions retrieval failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }
    }
}
