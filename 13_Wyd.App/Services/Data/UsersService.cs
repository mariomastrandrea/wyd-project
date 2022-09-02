using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Threading.Tasks;
using _13_Wyd.App.BusinessModels;
using _13_Wyd.ModelClasses;

namespace _13_Wyd.App.Services.Data
{
    public class UsersService
    {
        private readonly HttpClient DataClient;


        public UsersService(HttpClient dataClient)
        {
            this.DataClient = dataClient;
        }

        public async Task<IEnumerable<UserAccount>> GetUsers()
        {
            try
            {
                return await this.DataClient.GetFromJsonAsync<IEnumerable<UserAccount>>($"api/users");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<UserAccount> GetUser(string userId)
        {
            try
            {
                return await this.DataClient.GetFromJsonAsync<UserAccount>($"api/users/{userId}");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<BusinessUserAccount> GetShoppingUser(string userId)
        {
            try
            {
                //retrieve user
                UserAccount user = await this.DataClient.GetFromJsonAsync<UserAccount>($"api/users/{userId}");

                if (user == null) return null;

                //retrieve wishlist
                string serializedWishList = await this.DataClient.GetStringAsync($"api/users/wishlist/{userId}");

                if (serializedWishList == null) return null;

                Dictionary<Item, int> wishList =
                        JsonConvert.DeserializeObject<Dictionary<Item, int>>(serializedWishList);

                //retrieve shopping cart
                string serializedShoppingCart =
                        await this.DataClient.GetStringAsync($"api/users/shoppingcart/{userId}");

                if (serializedShoppingCart == null) return null;

                Dictionary<Item, int> shoppingCart =
                        JsonConvert.DeserializeObject<Dictionary<Item, int>>(serializedShoppingCart);

                //create shoppingUser
                BusinessUserAccount shoppingUser = new BusinessUserAccount(user)
                {
                    WishList = wishList,
                    ShoppingCart = shoppingCart
                };

                return shoppingUser;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<bool> SetUserWishList(BusinessUserAccount businessUser)
        {
            if (businessUser == null || businessUser.Id == null)
                return false;

            string userId = businessUser.Id;

            try
            {
                string serializedWishList =
                   await this.DataClient.GetStringAsync($"api/users/wishlist/{userId}");

                if (serializedWishList == null)
                    return false;

                Dictionary<Item, int> wishList =
                        JsonConvert.DeserializeObject<Dictionary<Item, int>>(serializedWishList);

                businessUser.WishList = wishList;   //ok -> set
                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<bool> SetUserShoppingCart(BusinessUserAccount businessUser)   //ok
        {
            if (businessUser == null || businessUser.Id == null)
                return false;

            string userId = businessUser.Id;

            try
            {
                string serializedShoppingCart =
                   await this.DataClient.GetStringAsync($"api/users/shoppingcart/{userId}");

                if (serializedShoppingCart == null)
                    return false;

                Dictionary<Item, int> shoppingCart =
                        JsonConvert.DeserializeObject<Dictionary<Item, int>>(serializedShoppingCart);

                businessUser.ShoppingCart = shoppingCart;   //ok -> set
                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<bool> SetUserReviews(BusinessUserAccount businessUser)    //ok
        {
            if (businessUser == null || businessUser.Id == null)
                return false;

            string userId = businessUser.Id;

            try
            {
                List<Review> reviews =
                   await this.DataClient.GetFromJsonAsync<List<Review>>($"api/users/reviews/{userId}");

                if (reviews == null)
                    return false;   //an error occured

                businessUser.Reviews = reviews;   //ok -> set
                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<UserAccount> CreateUser(UserAccount newUser)
        {
            try
            {
                var response = await this.DataClient.PostAsJsonAsync("api/users", newUser);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return null;
                }

                UserAccount userCreated = await response.Content.ReadFromJsonAsync<UserAccount>();
                return userCreated;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<bool> ClearShoppingCartOf(string userId)  //ok
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            try
            {
                var response = await this.DataClient.DeleteAsync($"api/users/shoppingcart/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return false;
                }

                bool clearedShoppingCart = await response.Content.ReadFromJsonAsync<bool>();
                return clearedShoppingCart;
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<UserAccount> UpdateUser(UserAccount updatedUser)  //ok
        {
            if (updatedUser == null || updatedUser.Id == null)
                return null;

            try
            {
                var response = await this.DataClient.PutAsJsonAsync($"api/users", updatedUser);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return null;
                }

                UserAccount responseUser = await response.Content.ReadFromJsonAsync<UserAccount>();
                return responseUser;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<bool> UpdateUserWishList(BusinessUserAccount businessUser)    //ok
        {
            if (businessUser == null || businessUser.WishList == null || businessUser.Id == null)
                return false;

            Dictionary<Item, int> wishlist = businessUser.WishList;
            string serializedWishList = JsonConvert.SerializeObject(wishlist);

            try
            {
                var response = await this.DataClient.PutAsJsonAsync(
                    $"api/users/wishlist/{businessUser.Id}", serializedWishList);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return false;
                }

                //Dictionary<Item,int> in JSON
                string serializedUpdatedWishList = await response.Content.ReadAsStringAsync();

                if (serializedWishList == null) return false;

                Dictionary<Item, int> updatedWishList =
                    JsonConvert.DeserializeObject<Dictionary<Item, int>>(serializedWishList);

                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<bool> UpdateUserShoppingCart(BusinessUserAccount businessUser)    //ok
        {
            if (businessUser == null || businessUser.ShoppingCart == null || businessUser.Id == null)
                return false;

            Dictionary<Item, int> shoppingCart = businessUser.ShoppingCart;
            string serializedShoppingCart = JsonConvert.SerializeObject(shoppingCart);

            try
            {
                var response = await this.DataClient.PutAsJsonAsync(
                    $"api/users/shoppingcart/{businessUser.Id}", serializedShoppingCart);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return false;
                }

                //Dictionary<Item, int> in JSON
                string serializedUpdatedShoppingCart = await response.Content.ReadAsStringAsync();

                if (serializedShoppingCart == null) return false;

                Dictionary<Item, int> updatedShoppingCart =
                    JsonConvert.DeserializeObject<Dictionary<Item, int>>(serializedShoppingCart);

                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<bool> UpdateUserReviews(BusinessUserAccount businessUser) //ok
        {
            if (businessUser == null || businessUser.Reviews == null || businessUser.Id == null)
                return false;

            List<Review> reviews = businessUser.Reviews;

            try
            {
                var response = await this.DataClient.PutAsJsonAsync($"api/users/reviews/{businessUser.Id}", reviews);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return false;
                }

                List<Review> updatedReviews = await response.Content.ReadFromJsonAsync<List<Review>>();
                return updatedReviews != null;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<bool> UpdateShoppingUser(BusinessUserAccount businessUser)    //ok
        {
            bool updated1 = await this.UpdateUserWishList(businessUser);
            bool updated2 = await this.UpdateUserShoppingCart(businessUser);

            return updated1 && updated2;
        }

        public async Task<UserAccount> DeleteUser(string userId)
        {
            try
            {
                var response = await this.DataClient.DeleteAsync($"api/users/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return null;
                }

                UserAccount deletedUser = await response.Content.ReadFromJsonAsync<UserAccount>();
                return deletedUser;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<IEnumerable<UserAccount>> SearchUsers(string name)
        {
            try
            {
                IEnumerable<UserAccount> searchedUsers =
                    await this.DataClient.GetFromJsonAsync<IEnumerable<UserAccount>>($"api/users/search?name={name}");

                return searchedUsers;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }
    }
}
