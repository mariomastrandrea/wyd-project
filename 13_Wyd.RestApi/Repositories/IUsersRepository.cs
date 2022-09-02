using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;

namespace _13_Wyd.RestApi.Repositories
{
    public interface IUsersRepository
    {
        Task<IEnumerable<UserAccount>> GetUsers();
        Task<UserAccount> GetUser(string userId);
        Task<UserAccount> GetUserByEmail(string userEmail);
        Task<IEnumerable<UserAccount>> SearchUsers(string name);
        Task<UserAccount> CreateUser(UserAccount newUser);
        Task<UserAccount> DeleteUser(string userId);
        Task<UserAccount> UpdateUser(UserAccount newUser);
        Task<Dictionary<Item, int>> GetUserShoppingCart(string id); 
        Task<Dictionary<Item, int>> GetUserWishList(string id); 
        Task<Dictionary<Item, int>> UpdateUserWishList(string id,
                                    Dictionary<Item, int> wishList); 
        Task<Dictionary<Item, int>> UpdateUserShoppingCart(string id,
                                    Dictionary<Item, int> shoppingCart);
        Task<List<Review>> GetUserReviews(string id);   
        Task<List<Review>> UpdateUserReviews(string id, List<Review> reviews); 
        Task<bool?> ClearShoppingCartOf(UserAccount user);        
        Task<Order> GetUserOrder(string userId, string orderId);
        Task<IEnumerable<Order>> GetUserOrders(string userId);
    }
}
