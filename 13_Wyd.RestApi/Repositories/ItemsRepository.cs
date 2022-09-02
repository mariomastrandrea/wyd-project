using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.RestApi.DB.Tables.Items;
using _13_Wyd.RestApi.DB.Tables.Reviews;
using _13_Wyd.RestApi.DB.Tables.Users;

namespace _13_Wyd.RestApi.Repositories
{
    public class ItemsRepository : IItemsRepository
    {
        private readonly ItemsTS ItemsTable;
        private readonly ItemAvgScoresTS AvgScoresTable;
        private readonly ItemQuantitiesTS QuantitiesTable;
        private readonly ItemInterestedUsersTS InterestedUsersTable;

        private readonly ReviewsTS ReviewsTable;
        private readonly  UsersTS UsersTable;


        public ItemsRepository(ItemsTS itemsTable, ItemAvgScoresTS avgScoresTable,
            ItemQuantitiesTS quantitiesTable, ItemInterestedUsersTS interestedUsersTable,
            ReviewsTS reviewsTable, UsersTS usersTable)
        {
            this.ItemsTable = itemsTable;
            this.AvgScoresTable = avgScoresTable;
            this.QuantitiesTable = quantitiesTable;
            this.InterestedUsersTable = interestedUsersTable;

            this.ReviewsTable = reviewsTable;

            this.UsersTable = usersTable;
        }

        public async Task<Item> GetItem(string itemId)
        {
            Item item = await this.ItemsTable.GetItem(itemId);
            return item;
        }

        public async Task<IEnumerable<Item>> GetItems()
        {
            IEnumerable<Item> allItems = await this.ItemsTable.GetItems();
            return allItems;
        }

        public async Task<IEnumerable<Item>> SearchItems(string name, decimal price)
        {
            IEnumerable<Item> searchedItems;

            if (name == null)
            {
                searchedItems = await this.ItemsTable.SearchItems(item => item.Price <= (double)price);
            }
            else
            {
                name = name.ToLower();

                searchedItems = await this.ItemsTable.SearchItems(item =>
                                    item.Price <= (double)price && (item.Name.ToLower().Contains(name)));
            }
            return searchedItems;
        }

        public async Task<IEnumerable<Item>> SearchItems(int? minScore, params string[] categories)
        {
            IEnumerable<Item> items;

            if (minScore != null)
            {
                Dictionary<string, decimal> itemsAvgScores =
                    await this.AvgScoresTable.FilterItemsByAvgScore(item => item.AvgScore >= (double)minScore);

                if (itemsAvgScores == null || !itemsAvgScores.Any())
                    return null;

                IEnumerable<string> itemIds = itemsAvgScores.Keys;
                items = await this.ItemsTable.GetItems(itemIds);
            }
            else
                items = await this.ItemsTable.GetItems();
            
            foreach (string c in categories)
            {
                if (items == null || !items.Any())
                    break;

                items = items.Where(item => item.Categories.Contains(c.ToLower()));
            }

            return items;
        }

        public async Task<Item> CreateItem(Item newItem)
        {
            if (newItem == null || !string.IsNullOrWhiteSpace(newItem.Id))
                return null;

            string newGuid = Guid.NewGuid().ToString();   //setting guid as itemId
            newItem.Id = $"IT-{newGuid}";

            IEnumerable<string> categories = newItem.Categories;
            if(categories != null && categories.Any())
            {
                newItem.Categories = categories.Select(c => c.ToLower());
            }

            Item createdItem = await this.ItemsTable.Create(newItem);

            int? qty = await this.QuantitiesTable.AddOrUpdateNewItemQty(newItem.Id, 0);
            double? avgScore = await this.AvgScoresTable.AddOrUpdateNewAvgScore(newItem.Id, 0.0);

            if (createdItem == null || qty == null || avgScore == null)
                return null;

            return createdItem;  
        }

        public async Task<Item> DeleteItem(string itemId)
        {
            if (itemId == null)
                return null;

            Item deletedItem = await this.ItemsTable.Delete(itemId);
            return deletedItem;
        }

        public async Task<Item> UpdateItem(Item newItem)
        {
            if (newItem == null || newItem.Id == null)
                return null;

            Item updatedItem = await this.ItemsTable.Update(newItem);
            return updatedItem;
        }

        public async Task<IEnumerable<Item>> UpdateItems(IEnumerable<Item> newItems)
        {
            if (newItems == null || !newItems.Any() || newItems.All(item => string.IsNullOrWhiteSpace(item.Id)))
                return null;

            IEnumerable<Item> updatedItems = await this.ItemsTable.UpdateAll(newItems);
            return updatedItems;
        }

        public async Task<int?> GetItemQty(string itemId)
        {
            if (itemId == null) return null;

            int? qty = await this.QuantitiesTable.GetQty(itemId);
            return qty;
        }

        public async Task<List<UserAccount>> GetItemInterestedUsers(string itemId)
        {
            if (itemId == null)
                return null;

            IEnumerable<string> interestedUsersIDs =
                                   await this.InterestedUsersTable.GetInterestedUsersIDs(itemId);

            IEnumerable<UserAccount> interestedUsers = await this.UsersTable.GetUsers(interestedUsersIDs);

            return interestedUsers?.ToList();
        }

        public async Task<List<UserAccount>> UpdateItemInterestedUsers(string itemId,
            List<UserAccount> interestedUsers)
        {
            if (itemId == null || interestedUsers == null)
                return null;

            IEnumerable<string> interestedUsersIDs = interestedUsers.Where(user => user.Id != null)
                                                                    .Select(user => user.Id);

            IEnumerable<string> updatedInterestedUsersIDs =
                            await this.InterestedUsersTable.Update(itemId, interestedUsersIDs);

            IEnumerable<UserAccount> updatedInterestedUsers =
                updatedInterestedUsersIDs.Select(id => interestedUsers.Find(user => user.Id.Equals(id)));

            return updatedInterestedUsers.ToList();
        }

        public async Task<List<Review>> GetItemReviews(string itemId)
        {
            if (itemId == null)
                return null;

            IEnumerable<Review> itemReviews = await this.ReviewsTable.GetItemReviews(itemId);

            return itemReviews?.ToList();
        }

        public async Task<List<Review>> UpdateItemReviews(string itemId, List<Review> reviews)
        {
            if (itemId == null || reviews == null)
                return null;

            IEnumerable<Review> updatedReviews =
                await this.ReviewsTable.UpdateItemReviews(itemId, reviews);

            return updatedReviews?.ToList();
        }

        public async Task<double> GetItemAvgScore(string itemId)
        {
            if (itemId == null)
                return double.NaN;

            double avgScore = await this.AvgScoresTable.GetAvgScoreOf(itemId) ?? double.NaN;

            return avgScore;
        }

        public async Task<double> UpdateItemAvgScore(string itemId, double avgScore)
        {
            if (itemId == null || double.IsNaN(avgScore)) return double.NaN;

            double updatedAvgScore =
                await this.AvgScoresTable.UpdateAvgScoreOf(itemId, avgScore) ?? double.NaN;

            return updatedAvgScore;
        }

        public async Task<Dictionary<string, int>> UpdateItemsQuantities(
                                    Dictionary<string, int> newQuantitiesByItemId)
        {
            if (newQuantitiesByItemId == null || !newQuantitiesByItemId.Any())
                return null;

            Dictionary<string, int> updatedItemsQuantities =
                    await this.QuantitiesTable.UpdateQuantities(newQuantitiesByItemId);

            return updatedItemsQuantities;
        }
    }
}
