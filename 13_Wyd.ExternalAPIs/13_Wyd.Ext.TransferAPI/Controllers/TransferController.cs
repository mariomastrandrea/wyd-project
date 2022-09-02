using System;
using _13_Wyd.ModelClasses.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _13_Wyd.Ext.TransferAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferController : ControllerBase
    {
        private readonly TransferPaymentManager Manager;


        public TransferController(TransferPaymentManager manager)
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
                bool ok = this.Manager.ProcessTransferPayment(paymentInfo);

                if (!ok)
                    return StatusCode(StatusCodes.Status406NotAcceptable, "Transfer Payment failed");

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
