using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using _13_Wyd.App.BusinessModels;
using _13_Wyd.ModelClasses;

namespace _13_Wyd.App.Services.Data
{
    public class ItemsService
    {
        private readonly HttpClient DataClient;


        public ItemsService(HttpClient dataClient)
        {
            this.DataClient = dataClient;
        }

        public async Task<IEnumerable<Item>> GetItems()
        {
            try
            {
                return await this.DataClient.GetFromJsonAsync<IEnumerable<Item>>($"api/items");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<Item> GetItem(string itemId)
        {
            try
            {
                return await this.DataClient.GetFromJsonAsync<Item>($"api/items/{itemId}");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<bool> SetItemQty(BusinessItem businessItem)   //ok
        {
            if (businessItem == null || businessItem.Id == null)
                return false;

            string itemId = businessItem.Id;

            try
            {
                int qty = await this.DataClient.GetFromJsonAsync<int>($"api/items/qty/{itemId}");

                businessItem.QtyInStock = qty;
                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<bool> SetInterestedUsers(BusinessItem businessItem)   //ok
        {
            if (businessItem == null || businessItem.Id == null)
                return false;

            string itemId = businessItem.Id;

            try
            {
                List<UserAccount> interestedUsers =
                    await this.DataClient.GetFromJsonAsync<List<UserAccount>>($"api/items/interestedusers/{itemId}");

                businessItem.InterestedUsers = interestedUsers;
                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<bool> SetItemReviewsAndScore(BusinessItem businessItem)   //ok
        {
            if (businessItem == null || businessItem.Id == null)
                return false;

            string itemId = businessItem.Id;

            try
            {
                List<Review> reviews =
                    await this.DataClient.GetFromJsonAsync<List<Review>>($"api/items/reviews/{itemId}");

                double avgScore = await this.DataClient.GetFromJsonAsync<double>($"api/items/score/{itemId}");

                businessItem.Reviews = reviews;
                businessItem.AvgScore = avgScore;
                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<bool> SetQtyAndInterestedUsers(IEnumerable<BusinessItem> businessItems)   //ok
        {
            if (businessItems == null || !businessItems.Any())    
                return false;

            bool allSetted = true;

            foreach(BusinessItem businessItem in businessItems)
            {
                bool setted1 = await this.SetItemQty(businessItem);
                if (!setted1)
                    businessItem.QtyInStock = null;

                bool setted2 = await this.SetInterestedUsers(businessItem);
                if (!setted2)
                    businessItem.InterestedUsers = null;

                allSetted &= (setted1 && setted2);
            }

            return allSetted;
        }

        public async Task<Item> CreateItem(Item newItem)    //ok
        {
            try
            {
                var response = await this.DataClient.PostAsJsonAsync("api/items", newItem);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return null;
                }

                Item itemCreated = await response.Content.ReadFromJsonAsync<Item>();
                return itemCreated;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<Item> UpdateItem(Item updatedItem)    //ok
        {
            var response = await this.DataClient.PutAsJsonAsync($"api/items", updatedItem);

            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                    $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                return null;
            }

            try
            {
                Item responseItem = await response.Content.ReadFromJsonAsync<Item>();
                return responseItem;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<bool> UpdateInterestedUsers(BusinessItem businessItem)    //ok
        {
            if (businessItem == null || businessItem.Id == null || businessItem.InterestedUsers == null)
                return false;

            List<UserAccount> interestedUsers = businessItem.InterestedUsers;

            try
            {
                var response = await this.DataClient.PutAsJsonAsync(
                                            $"api/items/interestedusers/{businessItem.Id}", interestedUsers);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return false;
                }

                List<UserAccount> updatedInterestedUsers = await response.Content.ReadFromJsonAsync<List<UserAccount>>();
                return updatedInterestedUsers != null;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<bool> UpdateItemsQuantities(IEnumerable<BusinessItem> businessItems)  //ok
        {
            if (businessItems == null || !businessItems.Any())
                return false;

            Dictionary<string, int> newItemQuantities =
                    businessItems.ToDictionary(x => x.Id, x => (int)x.QtyInStock);

            try
            {
                var response = await this.DataClient.PutAsJsonAsync(
                                            "api/items/quantities", newItemQuantities);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return false;
                }

                Dictionary<string, int> updatedItemQuantities =
                            await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();

                return updatedItemQuantities != null;
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<bool> UpdateItemReviewsAndScore(BusinessItem businessItem)    //ok
        {
            if (businessItem == null || businessItem.Id == null || businessItem.Reviews == null
                || businessItem.AvgScore == null || double.IsNaN((double)businessItem.AvgScore))
                return false;

            try
            {
                List<Review> reviews = businessItem.Reviews;

                var response = await this.DataClient.PutAsJsonAsync(
                                                $"api/items/reviews/{businessItem.Id}", reviews);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return false;
                }

                List<Review> updatedItemReviews = await response.Content.ReadFromJsonAsync<List<Review>>();

                if (updatedItemReviews == null)
                    return false;

                double avgScore = (double)businessItem.AvgScore;

                var response2 = await this.DataClient.PutAsJsonAsync(
                                                $"api/items/score/{businessItem.Id}", avgScore);

                if (!response2.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return false;
                }

                double updatedAvgScore = await response.Content.ReadFromJsonAsync<double>();

                if (double.IsNaN(updatedAvgScore))
                    return false;

                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<Item> DeleteItem(string itemId)   //ok
        {
            try
            {
                var response = await this.DataClient.DeleteAsync($"api/items/{itemId}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return null;
                }

                Item deletedItem = await response.Content.ReadFromJsonAsync<Item>();
                return deletedItem;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<IEnumerable<Item>> SearchItems(string name, int? maxPrice)
        {
            if (name == null && (maxPrice == null || maxPrice <= 0))
                return null;

            try
            {
                IEnumerable<Item> searchedItems;

                if (name == null)
                    searchedItems = await this.DataClient.GetFromJsonAsync<IEnumerable<Item>>(
                            $"api/items/search?price={maxPrice}");

                else if(maxPrice == null || maxPrice <= 0)
                    searchedItems = await this.DataClient.GetFromJsonAsync<IEnumerable<Item>>(
                            $"api/items/search?name={name}");

                else
                    searchedItems = await this.DataClient.GetFromJsonAsync<IEnumerable<Item>>(
                        $"api/items/search?name={name}&price={maxPrice}");

                return searchedItems;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<IEnumerable<Item>> SearchItems(int minScore, params string[] categories)
        {
            try
            {
                IEnumerable<Item> searchedItems;

                if (!categories.Any())
                    searchedItems = await this.DataClient.GetFromJsonAsync<IEnumerable<Item>>(
                        $"api/items/score/search?score={minScore}");

                else
                {
                    StringBuilder categoriesString = new StringBuilder();

                    foreach (string category in categories)
                        categoriesString.Append(category);

                    searchedItems = await this.DataClient.GetFromJsonAsync<IEnumerable<Item>>(
                            $"api/items/score/search?score={minScore}&categories={categoriesString}");
                }

                return searchedItems;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<IEnumerable<Item>> UpdateItems(IEnumerable<Item> updatedItems)    //unused
        {
            var response = await this.DataClient.PutAsJsonAsync($"api/items/many", updatedItems);

            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                    $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                return null;
            }

            try
            {
                IEnumerable<Item> responseItems = await response.Content.ReadFromJsonAsync<IEnumerable<Item>>();
                return responseItems;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }
    }
}
