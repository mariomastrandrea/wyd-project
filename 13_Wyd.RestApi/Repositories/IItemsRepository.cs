using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;

namespace _13_Wyd.RestApi.Repositories
{
    public interface IItemsRepository
    {
        Task<IEnumerable<Item>> GetItems();
        Task<Item> GetItem(string itemId);
        Task<IEnumerable<Item>> SearchItems(string name, decimal price);
        Task<IEnumerable<Item>> SearchItems(int? minScore, params string[] categories);
        Task<Item> CreateItem(Item newItem);
        Task<Item> DeleteItem(string itemId);
        Task<Item> UpdateItem(Item newItem);
        Task<IEnumerable<Item>> UpdateItems(IEnumerable<Item> newItems);
        Task<int?> GetItemQty(string itemId);   
        Task<List<UserAccount>> GetItemInterestedUsers(string itemId); 
        Task<List<UserAccount>> UpdateItemInterestedUsers(string itemId,
                                List<UserAccount> interestedUsers);  
        Task<List<Review>> GetItemReviews(string itemId);                           
        Task<List<Review>> UpdateItemReviews(string itemId, List<Review> reviews);  
        Task<double> GetItemAvgScore(string itemId);                                
        Task<double> UpdateItemAvgScore(string itemId, double avgScore);            
        Task<Dictionary<string, int>> UpdateItemsQuantities(
                                Dictionary<string, int> newQuantitiesByItemId);     
    }
}
