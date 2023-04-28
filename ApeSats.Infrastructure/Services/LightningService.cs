using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Model.Response;
using ApeSats.Infrastructure.Helper;
using Grpc.Core;
using Lnrpc;
using Microsoft.Extensions.Configuration;

namespace ApeSats.Infrastructure.Services
{
    public class LightningService : ILightningService
    {
        private readonly IConfiguration _config;
        public LightningService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> CreateInvoice(long satoshis, string message)
        {
            var helper = new LightningHelper(_config);
            try
            {
                var adminInvoice = helper.CreateAdminInvoice(satoshis, message);
                var paymentRequest = adminInvoice.PaymentRequest;
                return paymentRequest;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<long> GetChannelBalance()
        {
            var helper = new LightningHelper(_config);
            var channelBalanceRequest = new ChannelBalanceRequest();
            try
            {
                var adminClient = helper.GetAdminClient();
                var response = adminClient.ChannelBalance(channelBalanceRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) }).Balance;
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<long> GetWalletBalance()
        {
            var helper = new LightningHelper(_config);
            var walletBalanceRequest = new WalletBalanceRequest();
            try
            {
                var adminClient = helper.GetAdminClient();
                var response = adminClient.WalletBalance(walletBalanceRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) }).TotalBalance;
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<InvoiceSettlementResponse> ListenForSettledInvoice()
        {
            var settledInvoiceResponse = new InvoiceSettlementResponse();
            try
            {
                var helper = new LightningHelper(_config);
                var txnReq = new InvoiceSubscription();
                var lookup = new LookupHtlcRequest();
                /*txnReq.AddIndex = 16;
                txnReq.SettleIndex = 16;*/

                var adminClient = helper.GetAdminClient();
                var settledInvoice = adminClient.SubscribeInvoices(txnReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) });

                using (var call = settledInvoice)
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var invoice = call.ResponseStream.Current;
                        if (invoice.State == Invoice.Types.InvoiceState.Settled)
                        {
                            Console.WriteLine(invoice.ToString());
                            var split = invoice.Memo.Split('/');
                            settledInvoiceResponse.PaymentRequest = invoice.PaymentRequest;
                            settledInvoiceResponse.IsKeysend = invoice.IsKeysend;
                            settledInvoiceResponse.Value = invoice.Value;
                            settledInvoiceResponse.Expiry = invoice.Expiry;
                            settledInvoiceResponse.Settled = invoice.Settled;
                            settledInvoiceResponse.SettledDate = invoice.SettleDate;
                            settledInvoiceResponse.SettledIndex = (long)invoice.SettleIndex;
                            settledInvoiceResponse.Private = invoice.Private;
                            settledInvoiceResponse.AmountInSat = invoice.AmtPaidSat;
                            settledInvoiceResponse.ArtId = int.Parse(split[0]);
                            settledInvoiceResponse.UserId = split[1];
                            return settledInvoiceResponse;
                        }
                    }
                }
                /*using (var settled = settledInvoice)
                {
                    var responseReaderTask = Task.Run(async () =>
                    {
                        var responses = settled.ResponseStream.Current;
                        settledInvoiceResponse.PaymentRequest = responses.PaymentRequest;
                        settledInvoiceResponse.IsKeysend = responses.IsKeysend;
                        settledInvoiceResponse.Value = responses.Value;
                        settledInvoiceResponse.Expiry = responses.Expiry;
                        settledInvoiceResponse.Settled = responses.Settled;
                        settledInvoiceResponse.SettledDate = responses.SettleDate;
                        settledInvoiceResponse.SettledIndex = (long)responses.SettleIndex;
                        settledInvoiceResponse.Private = responses.Private;
                        settledInvoiceResponse.AmountInSat = responses.AmtPaidSat;
                        Console.WriteLine(JsonConvert.SerializeObject(responses));
                    });
                }*/
                return settledInvoiceResponse;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<long> GetValueFromInvoice(string paymentRequest)
        {
            var helper = new LightningHelper(_config);
            var paymentReq = new PayReqString();
            try
            {
                var adminClient = helper.GetAdminClient();
                paymentReq.PayReq = paymentRequest;
                var decodedAdminPaymentReq = adminClient.DecodePayReq(paymentReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) });
                var value = decodedAdminPaymentReq.NumSatoshis;
                return value;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> SendLightning(string paymentRequest)
        {
            var helper = new LightningHelper(_config);
            var sendRequest = new SendRequest();
            var paymentReq = new PayReqString();
            var walletBalance = await GetWalletBalance();
            try
            {
                var adminClient = helper.GetAdminClient();
                paymentReq.PayReq = paymentRequest;
                var decodedAdminPaymentReq = adminClient.DecodePayReq(paymentReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) });
                if (walletBalance < decodedAdminPaymentReq.NumSatoshis)
                {
                    throw new ArgumentException("Unable to complete lightning payment. Insufficient funds");
                }
                sendRequest.Amt = decodedAdminPaymentReq.NumSatoshis;
                sendRequest.PaymentRequest = paymentRequest;
                var adminResponse = adminClient.SendPaymentSync(sendRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) });
                var result = adminResponse.PaymentError;
                return result;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<long> DecodePaymentRequest(string paymentRequest)
        {            
            var helper = new LightningHelper(_config);
            var paymentReq = new PayReqString();
            var walletBalance = await GetWalletBalance();
            try
            {
                var adminClient = helper.GetAdminClient();
                paymentReq.PayReq = paymentRequest;
                var decodedAdminPaymentReq = adminClient.DecodePayReq(paymentReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) });
                if (walletBalance < decodedAdminPaymentReq.NumSatoshis)
                {
                    throw new ArgumentException("Unable to complete lightning payment. Insufficient node balance. Please contact support");
                }
                var result = decodedAdminPaymentReq.NumSatoshis;
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
