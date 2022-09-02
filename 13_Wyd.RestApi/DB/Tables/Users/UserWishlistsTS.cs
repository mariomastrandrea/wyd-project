using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _13_Wyd.RestApi.DB.Entities.Users;
using AzureTableStorage.TableQueryAsync;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Configuration;

namespace _13_Wyd.RestApi.DB.Tables.Users
{
    public class UserWishlistsTS
    {
        private CloudTableClient TableStorageClient;
        private CloudTable Table;
        private readonly string TABLE_NAME;


        public UserWishlistsTS(CloudTableClient tableStorageClient, IConfiguration configuration)
        {
            this.TableStorageClient = tableStorageClient;
            TABLE_NAME = configuration.GetSection("TableStorage")
                                      .GetSection("Tables")
                                      .GetValue<string>("UserWishlists");

            this.Table = this.TableStorageClient.GetTableReference(TABLE_NAME);
            this.Table.CreateIfNotExists();
        }

        public async Task<Dictionary<string, int>> GetWishlistOf(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var query = this.Table.CreateQuery<UserItemEntity>()
                                    .Where(row => row.PartitionKey.Equals(userId))
                                    .AsTableQuery();

            IEnumerable<UserItemEntity> itemsQtyEntries = await query.ExecuteAsync();

            if (itemsQtyEntries == null) return null;

            Dictionary<string, int> userWishlist =
                itemsQtyEntries.ToDictionary(entry => entry.RowKey, entry => entry.Qty);

            return userWishlist;
        }

        public async Task<Dictionary<string, int>> Update(string userId,
            Dictionary<string, int> wishlistIDs)
        {
            if (string.IsNullOrWhiteSpace(userId) || wishlistIDs == null)
                return null;

            Dictionary<string, int> oldWishlistIDs = await this.GetWishlistOf(userId);

            return await UserItemsExtensions.Update(this.Table, userId,
                oldWishlistIDs, wishlistIDs);
        }
    }
}
