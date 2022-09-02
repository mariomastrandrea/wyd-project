using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses.Payment.Events;
using _13_Wyd.RestApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace _13_Wyd.RestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentsRepository PaymentsRepository;


        public PaymentsController(IPaymentsRepository paymentsRepository)
        {
            this.PaymentsRepository = paymentsRepository;
        }

        // GET: api/payments/queue
        [HttpGet("queue")]
        public async Task<ActionResult<string>> GetQueuePayments()  //ok
        {
            try
            {
                IEnumerable<PaymentEvent> payments =
                    await this.PaymentsRepository.GetQueuePayments();

                if (payments == null)
                    return NotFound();

                string serializedPayments = JsonConvert.SerializeObject(payments);

                return StatusCode(200, serializedPayments);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // GET api/payments?id={id}&orderId={orderId}
        [HttpGet]
        public async Task<ActionResult<string>> GetPayment(string id, string orderId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(orderId))
                    return BadRequest();

                PaymentEvent payment = await this.PaymentsRepository.GetPayment(id, orderId);

                if (payment == null) return NotFound();

                string serializedPayment = JsonConvert.SerializeObject(payment);

                return Ok(serializedPayment);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // GET: api/payments/order/{orderId}
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<string>> GetOrderPayments(string orderId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderId))
                    return BadRequest();

                IEnumerable<PaymentEvent> orderPayments =
                    await this.PaymentsRepository.GetOrderPayments(orderId);

                if (orderPayments == null)
                    return NotFound();

                string serializedOrderPayments = JsonConvert.SerializeObject(orderPayments);

                return StatusCode(200, serializedOrderPayments);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // POST api/payments
        [HttpPost]
        public async Task<ActionResult<string>> AddNewPayment(          //ok
                                [FromBody] string serializedNewPayment) 
        {
            try
            {
                if (string.IsNullOrWhiteSpace(serializedNewPayment)) return BadRequest();

                PaymentEvent newPayment = JsonConvert.DeserializeObject<PaymentEvent>(serializedNewPayment);

                if (!string.IsNullOrWhiteSpace(newPayment.PaymentId))
                {
                    ModelState.AddModelError("paymentId", "ID must be 'null' or empty in order to create a new object");
                    return BadRequest(ModelState);
                }

                PaymentEvent paymentAdded = await this.PaymentsRepository.AddNewPayment(newPayment);
                string serializedPaymentAdded = JsonConvert.SerializeObject(paymentAdded);

                return CreatedAtAction(nameof(GetPayment), new { id = paymentAdded.PaymentId }, serializedPaymentAdded);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error saving data in the database");
            }
        }

        // PUT api/payments
        [HttpPut]
        public async Task<ActionResult<string>> SetPaymentProcessed(        //ok
                                                [FromBody] string serializedProcessedEvent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(serializedProcessedEvent))
                    return BadRequest();

                PaymentEvent processedEvent = JsonConvert.DeserializeObject<PaymentEvent>(serializedProcessedEvent);

                if (string.IsNullOrWhiteSpace(processedEvent.PaymentId) ||
                    processedEvent.Processed == false)  //PaymentEvent must already have this bool set
                    return BadRequest();

                PaymentEvent updatedPayment =
                    await this.PaymentsRepository.SetProcessedPayment(processedEvent);

                if (updatedPayment == null)
                {
                    ModelState.AddModelError("Id",
                        $"Payment with ID = {processedEvent.PaymentId} and orderID = {processedEvent.OrderId} not found");
                    return BadRequest(ModelState);
                }

                string serializedUpdatedPayment = JsonConvert.SerializeObject(updatedPayment);

                return Ok(serializedUpdatedPayment);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error updating data in the database");
            }
        }

        // DELETE api/payments?id={id}&orderId={orderId}
        [HttpDelete]
        public async Task<ActionResult<string>> DeletePayment(string id, string orderId)    
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(orderId))
                    return BadRequest();

                PaymentEvent oldPayment = await this.PaymentsRepository.DeletePayment(id, orderId);

                if (oldPayment == null)
                {
                    ModelState.AddModelError("paymentId", $"Payment with ID = {id} not found");
                    return BadRequest(ModelState);
                }

                string serializedOldPayment = JsonConvert.SerializeObject(oldPayment);

                return Ok(serializedOldPayment);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error deleting data in the database");
            }
        }
    }
}
