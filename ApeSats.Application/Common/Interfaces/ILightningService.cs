using ApeSats.Application.Common.Model.Response;
using ApeSats.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Application.Common.Interfaces
{
    public interface ILightningService
    {
        Task<string> CreateInvoice(long satoshis, string message);
        Task<long> GetChannelBalance();
        Task<long> GetWalletBalance();
        Task<long> DecodePaymentRequest(string paymentRequest);
        Task<string> SendLightning(string paymentRequest);
        Task<InvoiceSettlementResponse> ListenForSettledInvoice();
        Task<long> GetValueFromInvoice(string paymentRequest);
    }
}
