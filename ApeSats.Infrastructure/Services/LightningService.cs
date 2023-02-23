using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Common.Model.Response;
using ApeSats.Core.Enums;
using ApeSats.Infrastructure.Helper;
using Azure;
using Grpc.Core;
using Lnrpc;
using Microsoft.AspNetCore.Http.Extensions;
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

        public async Task<string> CreateInvoice(long satoshis, string message, UserType userType)
        {
            string paymentRequest = default;
            var helper = new LightningHelper(_config);
            try
            {
                switch (userType)
                {
                    case UserType.User:
                        var userInvoice = helper.CreateUserInvoice(satoshis, message);
                        paymentRequest = userInvoice.PaymentRequest;
                        break;
                    case UserType.Admin:
                        var adminInvoice = helper.CreateAdminInvoice(satoshis, message);
                        paymentRequest = adminInvoice.PaymentRequest;
                        break;
                    default:
                        break;
                }
                return paymentRequest;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<long> GetChannelBalance(UserType userType)
        {
            long response = default;
            var helper = new LightningHelper(_config);
            var channelBalanceRequest = new ChannelBalanceRequest();
            try
            {
                switch (userType)
                {
                    case UserType.User:
                        var userClient = helper.GetUserClient();
                        response = userClient.ChannelBalance(channelBalanceRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) }).Balance;
                        break;
                    case UserType.Admin:
                        var adminClient = helper.GetAdminClient();
                        response = adminClient.ChannelBalance(channelBalanceRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) }).Balance;
                        break;
                    default:
                        throw new ArgumentException("Invalid user type specified");
                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<long> GetWalletBalance(UserType userType)
        {
            var helper = new LightningHelper(_config);
            var walletBalanceRequest = new WalletBalanceRequest();
            long response = default;
            try
            {
                switch (userType)
                {
                    case UserType.User:
                        var userClient = helper.GetUserClient();
                        response = userClient.WalletBalance(walletBalanceRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) }).TotalBalance;
                        break;
                    case UserType.Admin:
                        var adminClient = helper.GetAdminClient();
                        response = adminClient.WalletBalance(walletBalanceRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) }).TotalBalance;
                        break;
                    default:
                        break;
                }
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<InvoiceSettlementResponse> ListenForSettledInvoice(UserType userType)
        {
            var settledInvoiceResponse = new InvoiceSettlementResponse();
            try
            {
                var helper = new LightningHelper(_config);
                var txnReq = new InvoiceSubscription();
                var lookup = new LookupHtlcRequest();
                /*txnReq.AddIndex = 16;
                txnReq.SettleIndex = 16;*/
                switch (userType)
                {
                    case UserType.User:
                        var userClient = helper.GetUserClient();
                        var settledInvoioce = userClient.SubscribeInvoices(txnReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                        using (var call = settledInvoioce)
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
                        break;
                    case UserType.Admin:
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
                        break;
                    default:
                        break;
                }
                return settledInvoiceResponse;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<long> GetValueFromInvoice(string paymentRequest, UserType userType)
        {
            long value = default;
            var helper = new LightningHelper(_config);
            var paymentReq = new PayReqString();
            try
            {
                switch (userType)
                {
                    case UserType.User:
                        var userClient = helper.GetUserClient();
                        paymentReq.PayReq = paymentRequest;
                        var decodedPaymentReq = userClient.DecodePayReq(paymentReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                        value = decodedPaymentReq.NumSatoshis;
                        break;
                    case UserType.Admin:
                        var adminClient = helper.GetAdminClient();
                        paymentReq.PayReq = paymentRequest;
                        var decodedAdminPaymentReq = adminClient.DecodePayReq(paymentReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) });
                        value = decodedAdminPaymentReq.NumSatoshis;
                        break;
                    default:
                        throw new ArgumentException("Invalid user type");
                        break;
                }
                return value;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> SendLightning(string paymentRequest, UserType userType)
        {
            string result = default;
            var helper = new LightningHelper(_config);
            var sendRequest = new SendRequest();
            var paymentReq = new PayReqString();
            var walletBalance = await GetWalletBalance(userType);
            try
            {
                switch (userType)
                {
                    case UserType.User:
                        var userClient = helper.GetUserClient();
                        paymentReq.PayReq = paymentRequest;
                        var decodedPaymentReq = userClient.DecodePayReq(paymentReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                        if (walletBalance < decodedPaymentReq.NumSatoshis)
                        {
                            throw new ArgumentException("Unable to complete lightning payment. Insufficient funds");
                        }
                        sendRequest.Amt = decodedPaymentReq.NumSatoshis;
                        sendRequest.PaymentRequest = paymentRequest;
                        var response = userClient.SendPaymentSync(sendRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                        result = response.PaymentError;
                        break;
                    case UserType.Admin:
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
                        result = adminResponse.PaymentError;
                        break;
                    default:
                        throw new ArgumentException("Invalid user type");
                        break;
                }
                return result;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<long> DecodePaymentRequest(string paymentRequest, UserType userType)
        {
            long result = default;
            var helper = new LightningHelper(_config);
            var paymentReq = new PayReqString();
            var walletBalance = await GetWalletBalance(userType);
            try
            {
                switch (userType)
                {
                    case UserType.User:
                        var userClient = helper.GetUserClient();
                        paymentReq.PayReq = paymentRequest;
                        var decodedPaymentReq = userClient.DecodePayReq(paymentReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                        if (walletBalance < decodedPaymentReq.NumSatoshis)
                        {
                            throw new ArgumentException("Unable to complete lightning payment. Insufficient node balance. Please contact support");
                        }
                        result = decodedPaymentReq.NumSatoshis;
                        break;
                    case UserType.Admin:
                        var adminClient = helper.GetAdminClient();
                        paymentReq.PayReq = paymentRequest;
                        var decodedAdminPaymentReq = adminClient.DecodePayReq(paymentReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) });
                        if (walletBalance < decodedAdminPaymentReq.NumSatoshis)
                        {
                            throw new ArgumentException("Unable to complete lightning payment. Insufficient node balance. Please contact support");
                        }
                        result = decodedAdminPaymentReq.NumSatoshis;
                        break;
                    default:
                        throw new ArgumentException("Invalid useer type specified");
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
