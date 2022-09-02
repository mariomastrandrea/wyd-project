using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.RestApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace _13_Wyd.RestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersRepository OrdersRepository;


        public OrdersController(IOrdersRepository ordersRepository)
        {
            this.OrdersRepository = ordersRepository;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            try
            {
                IEnumerable<Order> orders = await this.OrdersRepository.GetOrders();

                if (orders == null)
                    return NotFound();

                return StatusCode(200, orders);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // GET api/orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(string Id)
        {
            try
            {
                Order order = await this.OrdersRepository.GetOrder(Id);

                if (order == null)
                    return NotFound();

                return Ok(order);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // POST api/orders
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] Order newOrder)   //ok
        {
            try
            {
                if (newOrder == null)
                    return BadRequest();

                if (newOrder.Id != null && newOrder.Id.Length > 0)
                {
                    ModelState.AddModelError("orderId", "ID must be 'null' or empty in order to create a new object");
                    return BadRequest(ModelState);
                }

                Order order = await this.OrdersRepository.GetOrder(newOrder.Id);

                if (order != null)
                    return Conflict();

                Order orderCreated = await this.OrdersRepository.CreateOrder(newOrder);

                return CreatedAtAction(nameof(GetOrder), new { id = orderCreated.Id }, newOrder);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error saving data in the database");
            }
        }

        // PUT api/orders
        [HttpPut]
        public async Task<ActionResult<Order>> UpdateOrder([FromBody] Order order)
        {
            try
            {
                if (order == null || order.Id == null)
                    return BadRequest();

                Order updatedOrder = await this.OrdersRepository.UpdateOrder(order);

                if (updatedOrder == null)
                {
                    ModelState.AddModelError("orderId", $"Order with ID = {order.Id} not found");
                    return BadRequest(ModelState);
                }

                return Ok(updatedOrder);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error updating data in the database");
            }
        }

        // DELETE api/orders/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<Item>> DeleteOrder(string id)    //ok
        {
            try
            {
                Order oldOrder = await this.OrdersRepository.DeleteOrder(id);

                if (oldOrder == null)
                {
                    ModelState.AddModelError("orderId", $"Order with ID = {id} not found");
                    return BadRequest(ModelState);
                }

                return Ok(oldOrder);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error deleting data in the database");
            }
        }
    }
}
