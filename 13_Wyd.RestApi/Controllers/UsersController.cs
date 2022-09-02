using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.RestApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace _13_Wyd.RestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersRepository UsersRepository;


        public UsersController(IUsersRepository usersRepository)
        {
            this.UsersRepository = usersRepository;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserAccount>>> GetUsers()    //ok
        {
            try
            {
                IEnumerable<UserAccount> users = await this.UsersRepository.GetUsers();

                if (users == null || !users.Any())
                    return NotFound();

                return StatusCode(200, users);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // GET api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserAccount>> GetUser(string id) //ok
        {
            try
            {
                UserAccount user = await this.UsersRepository.GetUser(id);

                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // GET api/users/shoppingcart/{id}
        [HttpGet("shoppingcart/{id}")]              
        public async Task<ActionResult<string>> GetUserShoppingCart(string id)   //ok
        {
            try
            {
                Dictionary<Item, int> shoppingCart = await this.UsersRepository.GetUserShoppingCart(id);

                if (shoppingCart == null)
                    return NotFound();

                // Dictionary<Item, int> --> string
                string serializedShoppingCart = JsonConvert.SerializeObject(shoppingCart);

                return Ok(serializedShoppingCart);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // GET api/users/wishlist/{id}
        [HttpGet("wishlist/{id}")]
        public async Task<ActionResult<string>> GetUserWishList(string id)   //ok
        {
            try
            {
                Dictionary<Item, int> wishlist = await this.UsersRepository.GetUserWishList(id);

                if (wishlist == null)
                    return NotFound();

                // Dictionary<Item, int> --> string
                string serializedWishlist = JsonConvert.SerializeObject(wishlist);

                return Ok(serializedWishlist);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // GET api/users/reviews/{id}
        [HttpGet("reviews/{id}")]
        public async Task<ActionResult<List<Review>>> GetUserReviews(string id)     //ok
        {
            try
            {
                List<Review> reviews = await this.UsersRepository.GetUserReviews(id);

                if (reviews == null)
                    return NotFound();

                return Ok(reviews);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        //GET api/users/orders?userId={id}&orderId={id}
        [HttpGet("orders")]
        public async Task<ActionResult<Order>> GetUserOrder(string userId, string orderId)  //ok
        {
            if (userId == null || orderId == null)
                return BadRequest();

            try
            {
                UserAccount user = await this.UsersRepository.GetUser(userId);

                if (user == null)
                {
                    ModelState.AddModelError("userId", $"User with ID = {userId} not found");
                    return NotFound(ModelState);
                }

                Order userOrder = await this.UsersRepository.GetUserOrder(user.Id, orderId);

                if (userOrder == null)
                {
                    ModelState.AddModelError("orderId", $"User with ID = {userId} has no order with ID = {orderId}");
                    return NotFound(ModelState);
                }

                return Ok(userOrder);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        //GET api/users/orders/{userId}
        [HttpGet("orders/{userId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetUserOrders(string userId)
        {
            if (userId == null)
                return BadRequest();

            try
            {
                UserAccount user = await this.UsersRepository.GetUser(userId);

                if (user == null)
                {
                    ModelState.AddModelError("userId", $"User with ID = {userId} not found");
                    return NotFound(ModelState);
                }

                IEnumerable<Order> userOrders = await this.UsersRepository.GetUserOrders(user.Id);

                if (userOrders == null)
                    return NotFound();

                return Ok(userOrders);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // POST api/users
        [HttpPost]
        public async Task<ActionResult<UserAccount>> CreateUser([FromBody] UserAccount newUser) //ok
        {
            try
            {
                if (newUser == null)
                    return BadRequest();

                if (newUser.Id != null && newUser.Id.Length > 0)
                {
                    ModelState.AddModelError("userId", "ID must be 'null' or empty in order to create a new object");
                    return BadRequest(ModelState);
                }

                UserAccount user = await this.UsersRepository.GetUserByEmail(newUser.Email);

                if (user != null)
                {
                    ModelState.AddModelError("email", "user email already in use");
                    return Conflict(ModelState);
                }

                UserAccount userCreated = await this.UsersRepository.CreateUser(newUser);

                return CreatedAtAction(nameof(GetUser), new { id = userCreated.Id }, newUser);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error saving data in the database");
            }
        }

        //DELETE api/users/shoppingcart/{userId}
        [HttpDelete("shoppingcart/{userId}")]
        public async Task<ActionResult<bool>> ClearUserShoppingCart(string userId)      //ok                                                  
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return BadRequest();

                UserAccount user = await this.UsersRepository.GetUser(userId);

                if (user == null)
                {
                    ModelState.AddModelError("buyerId", $"User with ID = {userId} does not exist");
                    return BadRequest(ModelState);
                }

                bool? clearedShoppingCart = await this.UsersRepository.ClearShoppingCartOf(user);

                if(clearedShoppingCart == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                            $"Error clearing user {user.Id} shopping cart");
                }

                return Ok(clearedShoppingCart);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            $"Error clearing user {userId} shopping cart");
            }
        }

        // PUT api/users
        [HttpPut]
        public async Task<ActionResult<UserAccount>> UpdateUser([FromBody] UserAccount user)    //ok
        {
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Id))
                    return BadRequest();

                UserAccount userToUpdate = await this.UsersRepository.GetUser(user.Id);

                if (userToUpdate == null)
                {
                    ModelState.AddModelError("userId", $"User with ID = {user.Id} not found");
                    return BadRequest(ModelState);
                }

                if(user.HasSameFieldsOf(userToUpdate))
                {
                    ModelState.AddModelError("user", $"Already exists user with ID = {userToUpdate.Id}, " +
                        $"Email = {userToUpdate.Email}, First Name = {userToUpdate.FirstName}, LastName = {userToUpdate.LastName} and the same password");
                    return BadRequest(ModelState);
                }
               
                UserAccount u = await this.UsersRepository.GetUserByEmail(user.Email);

                if (u != null)
                {
                    ModelState.AddModelError("email", "user email already in use");
                    return Conflict(ModelState);
                }

                UserAccount updatedUser = await this.UsersRepository.UpdateUser(user);

                if (updatedUser == null)
                    return BadRequest();

                return Ok(updatedUser);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error updating data in the database");
            }
        }

        // PUT api/users/wishlist/{id}
        [HttpPut("wishlist/{id}")]
        public async Task<ActionResult<string>> UpdateUserWishList(string id,   //ok
                                        [FromBody] string serializedWishList)
        {
            try
            {
                if (serializedWishList == null)
                    return BadRequest();

                Dictionary<Item, int> wishlist;
                try
                {
                    wishlist = JsonConvert.DeserializeObject<Dictionary<Item, int>>(serializedWishList);
                }
                catch(JsonException)
                {
                    ModelState.AddModelError("wishlist", $"impossible to deserialize this wishlist parameter");
                    return BadRequest(ModelState);
                }

                UserAccount u = await this.UsersRepository.GetUser(id);

                if (u == null)
                {
                    ModelState.AddModelError("userId", $"user with ID = {id} does not exist");
                    return BadRequest(ModelState);
                }

                Dictionary<Item, int> updatedWishList =
                            await this.UsersRepository.UpdateUserWishList(id, wishlist);

                string serializedUpdatedWishlist =
                    JsonConvert.SerializeObject(updatedWishList);

                return Ok(serializedUpdatedWishlist);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error updating data in the database");
            }
        }

        // PUT api/users/shoppingcart/{id}
        [HttpPut("shoppingcart/{id}")]
        public async Task<ActionResult<string>> UpdateUserShoppingCart(string id,   //ok
                                        [FromBody] string serializedShoppingCart)
        {
            try
            {
                if (serializedShoppingCart == null)
                    return BadRequest();

                Dictionary<Item, int> shoppingCart;
                try
                {
                    shoppingCart = JsonConvert.DeserializeObject<Dictionary<Item, int>>(serializedShoppingCart);
                }
                catch(JsonException)
                {
                    ModelState.AddModelError("shoppingcart", $"impossible to deserialize this shoppingcart parameter");
                    return BadRequest(ModelState);
                }

                UserAccount u = await this.UsersRepository.GetUser(id);

                if (u == null)
                {
                    ModelState.AddModelError("userId", $"user with ID = {id} does not exist");
                    return BadRequest(ModelState);
                }

                Dictionary<Item, int> updatedShoppingCart =
                            await this.UsersRepository.UpdateUserShoppingCart(id, shoppingCart);

                string serializedUpdatedShoppingCart =
                            JsonConvert.SerializeObject(updatedShoppingCart);

                return Ok(serializedUpdatedShoppingCart);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // PUT api/users/reviews/{id}
        [HttpPut("reviews/{id}")]
        public async Task<ActionResult<List<Review>>> UpdateUserReviews(string id,  //ok
                                        [FromBody] List<Review> reviews)
        {
            try
            {
                if (reviews == null || id == null)
                    return BadRequest();

                UserAccount u = await this.UsersRepository.GetUser(id);

                if (u == null)
                {
                    ModelState.AddModelError("userId", $"user with ID = {id} does not exist");
                    return BadRequest(ModelState);
                }

                List<Review> updatedReviews =
                            await this.UsersRepository.UpdateUserReviews(id, reviews);

                return Ok(updatedReviews);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error updating data in the database");
            }
        }

        // DELETE api/users/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<UserAccount>> DeleteUser(string id)  //ok
        {
            try
            {
                UserAccount oldUser = await this.UsersRepository.DeleteUser(id);

                if (oldUser == null)
                {
                    ModelState.AddModelError("userId", $"User with ID = {id} not found");
                    return BadRequest(ModelState);
                }

                return Ok(oldUser);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error deleting data in the database");
            }
        }

        // GET api/users/search?name={name}
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<UserAccount>>> SearchUsers(string name)
        {
            try
            {
                IEnumerable<UserAccount> matchingUsers = await this.UsersRepository.SearchUsers(name);

                if (matchingUsers == null || !matchingUsers.Any())
                    return NotFound($"There is no item matching name = {name}");

                return Ok(matchingUsers);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }
    }
}
