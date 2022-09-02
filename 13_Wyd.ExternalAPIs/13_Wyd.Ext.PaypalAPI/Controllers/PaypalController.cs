using System;
using _13_Wyd.ModelClasses.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace _13_Wyd.Ext.PaypalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaypalController : ControllerBase
    {
        private readonly PaypalPaymentManager Manager;


        public PaypalController(PaypalPaymentManager manager)
        {
            this.Manager = manager;
        }

        [HttpPost("pay")]
        public ActionResult HandlePayment([FromBody] PaymentInfo paymentInfo)
        {
            if (paymentInfo == null)
                return BadRequest();

            try
            {
                bool ok = this.Manager.ProcessPaypalPayment(paymentInfo);

                if (!ok)
                    return StatusCode(StatusCodes.Status406NotAcceptable, "Paypal Payment failed");

                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected internal server error occured");
            }
        }
    }
}
