using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.RestApi.DB.Tables.Orders;
using _13_Wyd.RestApi.DB.Tables.Items;
using _13_Wyd.RestApi.DB.Tables.Reviews;
using _13_Wyd.RestApi.DB.Tables.Users;

namespace _13_Wyd.RestApi.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly UsersTS UsersTable;
        private readonly UserShoppingCartsTS ShoppingCartsTable;
        private readonly UserWishlistsTS WishlistsTable;

        private readonly ItemsTS ItemsTable;
        private readonly ReviewsTS ReviewsTable;
        private readonly OrdersTS OrdersTable;


        public UsersRepository(UsersTS usersTable, UserShoppingCartsTS shoppingCartsTable,
            UserWishlistsTS wishlistsTable, ItemsTS itemsTable,
            ReviewsTS reviewsTable, OrdersTS ordersTable)
        {
            this.UsersTable = usersTable;
            this.ShoppingCartsTable = shoppingCartsTable;
            this.WishlistsTable = wishlistsTable;

            this.ItemsTable = itemsTable;
            this.ReviewsTable = reviewsTable;
            this.OrdersTable = ordersTable;
        }

        public async Task<UserAccount> GetUser(string userId)
        {
            if (userId == null)
                return null;

            UserAccount user = await this.UsersTable.GetUser(userId);
            return user;
        }

        public async Task<UserAccount> GetUserByEmail(string userEmail)
        {
            if (userEmail == null)
                return null;

            UserAccount user = await this.UsersTable.GetUserByEmail(userEmail);
            return user;
        }

        public async Task<IEnumerable<UserAccount>> GetUsers()
        {
            IEnumerable<UserAccount> allUsers = await this.UsersTable.GetUsers();
            return allUsers;
        }

        public async Task<IEnumerable<UserAccount>> SearchUsers(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            name = name.ToLower();

            IEnumerable<UserAccount> searchedUsers =
                await this.UsersTable.SearchUsers(user => user.FirstName.ToLower().Contains(name)
                                                       || user.LastName.ToLower().Contains(name));

            return searchedUsers;
        }

        public async Task<UserAccount> CreateUser(UserAccount newUser)
        {
            if (newUser == null || !string.IsNullOrEmpty(newUser.Id))
                return null;

            string newGuid = Guid.NewGuid().ToString();    //setting guid as userId
            newUser.Id = $"US-{newGuid}";
            
            UserAccount createdUser = await this.UsersTable.Create(newUser);
            return createdUser;
        }

        public async Task<UserAccount> DeleteUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            UserAccount removedUser = await this.UsersTable.Delete(userId);
            return removedUser;
        }

        public async Task<UserAccount> UpdateUser(UserAccount newUser)
        {
            if (newUser == null || newUser.Id == null)
                return null;

            UserAccount updatedUser = await this.UsersTable.Update(newUser);
            return updatedUser;
        }

        public async Task<Dictionary<Item, int>> GetUserShoppingCart(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            Dictionary<string, int> userShoppingCartIDs =
                await this.ShoppingCartsTable.GetShoppingCartOf(userId);

            IEnumerable<string> itemsIDs = userShoppingCartIDs.Keys;
            IEnumerable<Item> items = await this.ItemsTable.GetItems(itemsIDs);

            if (items == null) return null;

            Dictionary<Item, int> userShoppingCart =
                items.ToDictionary(item => item, item => userShoppingCartIDs[item.Id]);

            return userShoppingCart;
        }

        public async Task<Dictionary<Item, int>> GetUserWishList(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            Dictionary<string, int> userWishlistIDs =
                await this.WishlistsTable.GetWishlistOf(userId);

            IEnumerable<string> itemsIDs = userWishlistIDs.Keys;
            IEnumerable<Item> items = await this.ItemsTable.GetItems(itemsIDs);

            if (items == null) return null;

            Dictionary<Item, int> userWishlist =
                items.ToDictionary(item => item, item => userWishlistIDs[item.Id]);

            return userWishlist;
        }

        public async Task<Dictionary<Item, int>> UpdateUserWishList(string userId,
            Dictionary<Item, int> wishlist)
        {
            if (string.IsNullOrWhiteSpace(userId) || wishlist == null)
                return null;

            Dictionary<string, int> wishlistIDs =
                wishlist.ToDictionary(pair => pair.Key.Id, pair => pair.Value);

            Dictionary<string, int> updatedWishlistIDs =
                await this.WishlistsTable.Update(userId, wishlistIDs);

            Dictionary<Item, int> updatedWishlist =
                updatedWishlistIDs.ToDictionary(pair =>
                {
                    string itemId = pair.Key;
                    Item item = wishlist.Keys.FirstOrDefault(item => item.Id.Equals(itemId));
                    return item;
                },
                pair => pair.Value);

            return updatedWishlist;
        }

        public async Task<Dictionary<Item, int>> UpdateUserShoppingCart(string userId,
            Dictionary<Item, int> shoppingCart)
        {
            if (string.IsNullOrWhiteSpace(userId) || shoppingCart == null)
                return null;

            Dictionary<string, int> shoppingCartIDs =
                shoppingCart.ToDictionary(pair => pair.Key.Id, pair => pair.Value);

            Dictionary<string, int> updatedShoppingCartIDs =
                await this.ShoppingCartsTable.Update(userId, shoppingCartIDs);

            Dictionary<Item, int> updatedShoppingCart =
                updatedShoppingCartIDs.ToDictionary(pair =>
                {
                    string itemId = pair.Key;
                    Item item = shoppingCart.Keys.FirstOrDefault(item => item.Id.Equals(itemId));
                    return item;
                },
                pair => pair.Value);

            return updatedShoppingCart;
        }

        public async Task<List<Review>> GetUserReviews(string userId)
        {
            if (userId == null)
                return null;

            IEnumerable<Review> userReviews = await this.ReviewsTable.GetUserReviews(userId);

            if (userReviews == null)
                return null;

            return userReviews.ToList();
        }

        public async Task<List<Review>> UpdateUserReviews(string userId, List<Review> reviews)
        {
            if (string.IsNullOrWhiteSpace(userId) || reviews == null)
                return null;

            IEnumerable<Review> updatedUserReviews =
                await this.ReviewsTable.UpdateUserReviews(userId, reviews);

            if (updatedUserReviews == null) return null;

            return updatedUserReviews.ToList();
                
        }

        public async Task<bool?> ClearShoppingCartOf(UserAccount user)
        {
            if (user == null || user.Id == null) return false;

            bool? cleared = await this.ShoppingCartsTable.ClearFor(user.Id);
            return cleared;
        }

        public async Task<Order> GetUserOrder(string userId, string orderId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(orderId))
                return null;

            Order retrievedOrder = await this.OrdersTable.GetUserOrder(userId, orderId);

            return retrievedOrder;
        }

        public async Task<IEnumerable<Order>> GetUserOrders(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            IEnumerable<Order> userOrders = await this.OrdersTable.GetUserOrders(userId);

            return userOrders;
        }
    }
}
