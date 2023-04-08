using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using System;
using System.Threading.Tasks;

namespace EstiloMB.Core
{
    public class MercadoPagoPaymentProcessor
    {
        private string _accessToken;

        public MercadoPagoPaymentProcessor(string accessToken)
        {
            _accessToken = accessToken;
        }

        public async Task<Payment> CreatePayment(double transactionAmount, string token, string description, double installments, string paymentMethodId, string email, string identificationType, string identificationNumber)
        {
            MercadoPagoConfig.AccessToken = _accessToken;

            var paymentRequest = new PaymentCreateRequest
            {
                TransactionAmount = (decimal?)transactionAmount,
                Token = token,
                Description = description,
                Installments = (int?)installments,
                PaymentMethodId = paymentMethodId,
                Payer = new PaymentPayerRequest
                {
                    Email = "jhonevertonapp@outlook.com",
                    Identification = new IdentificationRequest
                    {
                        Type = identificationType,
                        Number = identificationNumber,
                    },
                },
            };

            var client = new PaymentClient();
            Payment payment = await client.CreateAsync(paymentRequest);
            Console.WriteLine(payment.Status);

            return payment;
        }
    }
}
