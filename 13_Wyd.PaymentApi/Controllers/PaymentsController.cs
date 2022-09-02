using System;
using System.Linq;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses.Payment.Events;
using _13_Wyd.PaymentApi.EventsPayment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace _13_Wyd.PaymentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentsManager PaymentsManager;


        public PaymentsController(PaymentsManager paymentsHandler)
        {
            this.PaymentsManager = paymentsHandler;
        }

        //POST: api/payments
        [HttpPost]
        public async Task<ActionResult<string>> HandlePayment([FromBody] string serializedPaymentEvent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(serializedPaymentEvent))
                    return BadRequest();

                PaymentEvent paymentEvent = JsonConvert.DeserializeObject<PaymentEvent>(serializedPaymentEvent);

                if (!this.CheckPaymentEvent(paymentEvent))
                    return BadRequest();

                PaymentEvent enqueuedPayment = await this.PaymentsManager.EnqueueNewPayment(paymentEvent);

                if (enqueuedPayment == null)
                    return StatusCode(StatusCodes.Status422UnprocessableEntity, null);

                string serializedEnqueuedPayment = JsonConvert.SerializeObject(enqueuedPayment);

                return Ok(serializedEnqueuedPayment);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected internal server error occured");
            }
        }

        //GET: api/payments/run
        [HttpGet("run")]
        public ActionResult<string> RunPayments()
        {
            try
            {
                if(PaymentsManager.On)
                    return BadRequest("Error: payments runner is already running!");

                PaymentsManager.On = true;

                //to be run in background
                Task.Run(async () => await this.PaymentsManager.RunPayments());   //check if it works

                return Ok("Payments runner started");
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected internal server error occured");
            }
        }

        //GET: api/payments/stop
        [HttpGet("stop")]
        public ActionResult<string> StopPayments()
        {
            try
            {
                if (!PaymentsManager.On)
                    return BadRequest("Error: Payments runner is already stopped!");
                
                PaymentsManager.On = false;
                return Ok("Payments runner stopped");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected internal server error occured");
            }
        }

        private bool CheckPaymentEvent(PaymentEvent paymentEvent)
        {
            if (paymentEvent == null ||
                !string.IsNullOrWhiteSpace(paymentEvent.PaymentId) || // -> paymentEvent must not have an ID here
                string.IsNullOrWhiteSpace(paymentEvent.UserId) ||
                string.IsNullOrWhiteSpace(paymentEvent.OrderId) ||
                paymentEvent.ItemsByQty == null || !paymentEvent.ItemsByQty.Any())
                return false;

            return true;
        }
    }
}