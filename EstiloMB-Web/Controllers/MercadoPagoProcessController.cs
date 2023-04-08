using Microsoft.AspNetCore.Mvc;
using EstiloMB.Core;
using System;
using System.Threading.Tasks;
using MercadoPago.Resource.Payment;
using EstiloMB.Site.Controllers;
using Newtonsoft.Json;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Common;

namespace EstiloMB.Site.Controllers
{
    public class MercadoPagoProcessController : Controller
    {

        public async Task<ActionResult<Payment>> ProcessPayment([FromBody] Object obj)
        {
            try
            {
                PaymentRequest paymentRequest = JsonConvert.DeserializeObject<PaymentRequest>(obj.ToString());
                MercadoPagoPaymentProcessor mercadoPagoPaymentProcessor = new("TEST-5364971616469083-032117-7b28fe0f5d401f5d8472b207dcf2305f-494665433");

                Payment payment = await mercadoPagoPaymentProcessor?.CreatePayment(
                    paymentRequest.TransactionAmount,
                    paymentRequest.Token,
                    paymentRequest.Description,
                    paymentRequest.Installments,
                    paymentRequest.PaymentMethodId,
                    paymentRequest.Email,
                    paymentRequest.IdentificationType,
                    paymentRequest.IdentificationNumber
                );

                if (payment != null)
                {
                    return Ok(payment);
                }
                else
                {
                    return BadRequest("Falha no processamento do pagamento.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
