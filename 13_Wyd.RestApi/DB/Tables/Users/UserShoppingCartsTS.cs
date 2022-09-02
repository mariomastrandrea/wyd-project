using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.RestApi.DB.Entities.Users;
using AzureTableStorage.TableQueryAsync;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Configuration;

namespace _13_Wyd.RestApi.DB.Tables.Users
{
    public class UserShoppingCartsTS
    {
        private CloudTableClient TableStorageClient;
        private CloudTable Table;
        private readonly string TABLE_NAME;


        public UserShoppingCartsTS(CloudTableClient tableStorageClient, IConfiguration configuration)
        {
            this.TableStorageClient = tableStorageClient;
            TABLE_NAME = configuration.GetSection("TableStorage")
                                      .GetSection("Tables")
                                      .GetValue<string>("UserShoppingCarts");

            this.Table = this.TableStorageClient.GetTableReference(TABLE_NAME);
            this.Table.CreateIfNotExists();
        }

        public async Task<Dictionary<string, int>> GetShoppingCartOf(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var query = this.Table.CreateQuery<UserItemEntity>()
                                    .Where(row => row.PartitionKey.Equals(userId))
                                    .AsTableQuery();

            IEnumerable<UserItemEntity> itemsQtyEntries = await query.ExecuteAsync();

            if (itemsQtyEntries == null) return null;

            Dictionary<string, int> userShoppingCart =
                itemsQtyEntries.ToDictionary(entry => entry.RowKey, entry => entry.Qty);

            return userShoppingCart;
        }

        public async Task<Dictionary<string, int>> Update(string userId,
            Dictionary<string, int> shoppingCartIDs)
        {
            if (string.IsNullOrWhiteSpace(userId) || shoppingCartIDs == null)
                return null;

            Dictionary<string, int> oldShoppingCartIDs = await this.GetShoppingCartOf(userId);

            return await UserItemsExtensions.Update(this.Table, userId,
                oldShoppingCartIDs, shoppingCartIDs);
        }

        public async Task<bool?> ClearFor(string userId)
        {
            if (userId == null || string.IsNullOrWhiteSpace(userId))
                return null;

            Dictionary<string, int> userShoppingCart =
                                await this.GetShoppingCartOf(userId);

            if (userShoppingCart == null) return null;
            if (!userShoppingCart.Any()) return false;   //already cleared

            TableBatchOperation deleteBatch = new TableBatchOperation();

            foreach(var pair in userShoppingCart)
            {
                UserItemEntity entityToDelete = new UserItemEntity(userId, pair.Key, pair.Value)
                {
                    ETag = "*"
                };

                TableOperation deleteOperation = TableOperation.Delete(entityToDelete);
                deleteBatch.Add(deleteOperation);
            }

            var result = await this.Table.ExecuteBatchAsync(deleteBatch);

            if (result == null || result.AsEnumerable().Any(tResult => tResult == null))    
                return null;   //some error occured

            return true;
        }
    }
}
