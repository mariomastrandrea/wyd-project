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
    public class ItemsController : ControllerBase
    {
        private readonly IItemsRepository ItemsRepository;


        public ItemsController(IItemsRepository itemsRepository)
        {
            this.ItemsRepository = itemsRepository;
        }

        // GET api/items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems()   //ok
        {
            try
            {
                IEnumerable<Item> items = await this.ItemsRepository.GetItems();

                if (items == null || !items.Any())
                    return NotFound();
                
                return StatusCode(200, items);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // GET api/items/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(string id)    //ok
        {
            try
            {
                Item item = await this.ItemsRepository.GetItem(id);

                if (item == null)
                    return NotFound();

                return Ok(item);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // GET api/items/qty/{id}
        [HttpGet("qty/{id}")]
        public async Task<ActionResult<int>> GetItemQty(string id)  //ok
        {
            try
            {
                int? qty = await this.ItemsRepository.GetItemQty(id);

                if (qty == null)
                    return NotFound();

                return Ok(qty);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // GET api/items/interestedusers/{id}
        [HttpGet("interestedusers/{id}")]
        public async Task<ActionResult<List<UserAccount>>> GetItemInterestedUsers(string id)    //ok
        {
            try
            {
                List<UserAccount> interestedUsers = await this.ItemsRepository.GetItemInterestedUsers(id);

                if (interestedUsers == null)
                    return NotFound();

                return Ok(interestedUsers);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // GET api/items/reviews/{id}
        [HttpGet("reviews/{id}")]
        public async Task<ActionResult<List<Review>>> GetItemReviews(string id) //ok
        {
            try
            {
                List<Review> reviews = await this.ItemsRepository.GetItemReviews(id);

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

        // GET api/items/score/{id}
        [HttpGet("score/{id}")]
        public async Task<ActionResult<double>> GetItemAvgScore(string id)  //ok
        {
            try
            {
                double avgScore = await this.ItemsRepository.GetItemAvgScore(id);

                if (double.IsNaN(avgScore))
                    return NotFound();

                return Ok(avgScore);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // POST api/items
        [HttpPost]
        public async Task<ActionResult<Item>> CreateItem([FromBody] Item newItem)   //ok
        {
            try
            {
                if (newItem == null)
                    return BadRequest();

                if (newItem.Id != null && newItem.Id.Length > 0)
                {
                    ModelState.AddModelError("itemId", "ID must be 'null' or empty in order to create a new object");
                    return BadRequest(ModelState);
                }

                Item itemCreated = await this.ItemsRepository.CreateItem(newItem);

                return CreatedAtAction(nameof(GetItem), new { id = itemCreated.Id }, newItem);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error saving data in the database");
            }
        }

        // PUT api/items
        [HttpPut]
        public async Task<ActionResult<Item>> UpdateItem([FromBody] Item item)  //ok
        {
            try
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id))
                    return BadRequest();

                Item itemToUpdate = await this.ItemsRepository.GetItem(item.Id);

                if (itemToUpdate == null)
                {
                    ModelState.AddModelError("itemId", $"Item with ID = {item.Id} not found");
                    return BadRequest(ModelState);
                }

                if(item.HasSameFieldsOf(itemToUpdate))
                {
                    ModelState.AddModelError("item", $"Already exists item with ID = {itemToUpdate.Id}; " +
                        $"Name = {itemToUpdate.Name}; Cost = {itemToUpdate.Cost}; Seller = {itemToUpdate.Seller};" +
                        $"Image = {itemToUpdate.Image}; Price = {itemToUpdate.Price}; Period = {itemToUpdate.PeriodTicks};" +
                        $"Categories = {string.Join(", ", itemToUpdate.Categories)}");
                    return BadRequest(ModelState);
                }

                Item updatedItem = await this.ItemsRepository.UpdateItem(item);

                if (updatedItem == null)
                    return BadRequest();

                return Ok(updatedItem);
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error updating data in the database");
            }
        }

        // PUT api/items/interestedusers/{id}
        [HttpPut("interestedusers/{id}")]
        public async Task<ActionResult<List<UserAccount>>> UpdateItemInterestedUsers(   //ok
                                            string id, [FromBody] List<UserAccount> interestedUsers)
        {
            try
            {
                if (interestedUsers == null)
                    return BadRequest();

                Item item = await this.ItemsRepository.GetItem(id);

                if (item == null)
                {
                    ModelState.AddModelError("itemId", $"Item with ID = {item.Id} not found");
                    return BadRequest(ModelState);
                }

                List<UserAccount> updatedInterestedUsers =
                    await this.ItemsRepository.UpdateItemInterestedUsers(id, interestedUsers);

                return Ok(updatedInterestedUsers);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error updating data in the database");
            }
        }

        // PUT api/items/reviews/{id}
        [HttpPut("reviews/{id}")]
        public async Task<ActionResult<List<Review>>> UpdateItemReviews(    //ok
                                            string id, [FromBody] List<Review> reviews)
        {
            try
            {
                if (reviews == null || id == null)
                    return BadRequest();

                Item item = await this.ItemsRepository.GetItem(id);

                if (item == null)
                {
                    ModelState.AddModelError("itemId", $"Item with ID = {item.Id} not found");
                    return BadRequest(ModelState);
                }

                List<Review> updatedItemReviews =
                    await this.ItemsRepository.UpdateItemReviews(id, reviews);

                return Ok(updatedItemReviews);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error updating data in the database");
            }
        }

        // PUT api/items/score/{id}
        [HttpPut("score/{id}")]
        public async Task<ActionResult<double>> UpdateItemAvgScore( //ok
                                            string id, [FromBody] double avgScore)
        {
            try
            {
                if (double.IsNaN(avgScore) || id == null)
                    return BadRequest();

                Item item = await this.ItemsRepository.GetItem(id);

                if (item == null)
                {
                    ModelState.AddModelError("itemId", $"Item with ID = {item.Id} not found");
                    return BadRequest(ModelState);
                }

                double updatedAvgScore =
                    await this.ItemsRepository.UpdateItemAvgScore(id, avgScore);

                return Ok(updatedAvgScore);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error updating data in the database");
            }
        }

        // PUT api/items/quantities
        [HttpPut("quantities")]
        public async Task<ActionResult<double>> UpdateItemsQuantities(  //ok
                                    [FromBody] Dictionary<string, int> newQuantitiesByItemId)
        {
            try
            {
                if (newQuantitiesByItemId == null || !newQuantitiesByItemId.Any())
                    return BadRequest();

                List<string> notExistingItems = new List<string>();

                foreach(var pair in newQuantitiesByItemId)
                {
                    string itemId = pair.Key;

                    Item i = await this.ItemsRepository.GetItem(itemId);

                    if (i == null)
                        notExistingItems.Add(itemId);
                }

                if(notExistingItems.Any())
                {
                    int count = 0;
                    foreach(string id in notExistingItems)
                    {
                        ModelState.AddModelError($"itemId{++count}", $"Item with ID = {id} not found");
                    }

                    return BadRequest(ModelState);
                }

                Dictionary<string, int> updatedQuantities =
                    await this.ItemsRepository.UpdateItemsQuantities(newQuantitiesByItemId);

                return Ok(updatedQuantities);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error updating data in the database");
            }
        }

        // PUT api/items/many
        [HttpPut("many")]
        public async Task<ActionResult<IEnumerable<Item>>> UpdateItems([FromBody] IEnumerable<Item> items)  //ok
        {
            try
            {
                if (items == null || !items.Any())
                    return BadRequest();

                if(items.Any(item => item.Id == null)) 
                    items = items.Where(item => item.Id != null);

                IEnumerable<Item> updatedItems = await this.ItemsRepository.UpdateItems(items);

                if (updatedItems == null || !updatedItems.Any())
                    return BadRequest($"All items not found");

                return Ok(updatedItems);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error updating data in the database");
            }
        }

        // DELETE api/items/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<Item>> DeleteItem(string id) //ok
        {
            try
            {
                Item oldItem = await this.ItemsRepository.DeleteItem(id);

                if (oldItem == null)
                {
                    ModelState.AddModelError("itemId", $"Item with ID = {id} not found");
                    return BadRequest(ModelState); 
                }

                return Ok(oldItem);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error deleting data in the database");
            }
        }

        //price --> maxPrice
        // GET api/items/search?name={name}&price={price}
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Item>>> SearchItems(string name, int? price)
        {
            try
            {
                decimal maxPrice;
                if (price == null || price <= 0)
                    maxPrice = decimal.MaxValue;
                else
                    maxPrice = new decimal((int)price);

                IEnumerable<Item> matchingItems = await this.ItemsRepository.SearchItems(name, maxPrice);

                if (matchingItems == null || !matchingItems.Any())
                {
                    string message = $"There is no item matching ";

                    if (name != null)
                        message += $"name = {name}";

                    if (price != null)
                    {
                        if (name != null)
                            message += " and ";

                        message += $"price = {price}";
                    }

                    return NotFound(message);
                }

                return Ok(matchingItems);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }

        // score --> min Score
        // GET api/items/score/search?score={score}&categories={category1,category2,....}
        [HttpGet("score/search")]
        public async Task<ActionResult<IEnumerable<Item>>> SearchItems(int? score, string categories)
        {
            try
            {
                IEnumerable<Item> matchingItems;

                if (categories == null || categories.Equals(string.Empty))
                    matchingItems = await this.ItemsRepository.SearchItems(score);
                else
                {
                    string[] arrayCategories = categories.Split(',');
                    matchingItems = await this.ItemsRepository.SearchItems(score, arrayCategories);
                }

                if (matchingItems == null || !matchingItems.Any())
                {
                    string message = "There is no item matching ";

                    if (score != null)
                        message += $"min score = {score}";

                    if (categories != null)
                    {
                        if (score != null)
                            message += " and ";

                        message += $"categories = {categories}";
                    }

                    return NotFound(message);
                }

                return Ok(matchingItems);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                            "Error retrieving data from the database");
            }
        }
    }
}
