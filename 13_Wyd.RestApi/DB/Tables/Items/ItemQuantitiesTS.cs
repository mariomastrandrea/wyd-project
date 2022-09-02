using System;
using Microsoft.Azure.Cosmos.Table;
using _13_Wyd.RestApi.DB.Entities.Items;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace _13_Wyd.RestApi.DB.Tables.Items
{
    public class ItemQuantitiesTS
    {
        private readonly CloudTableClient TableStorageClient;
        private readonly CloudTable Table;
        private readonly string TABLE_NAME;


        public ItemQuantitiesTS(CloudTableClient tableStorageClient, IConfiguration configuration)
        {
            this.TableStorageClient = tableStorageClient;
            TABLE_NAME = configuration.GetSection("TableStorage")
                                      .GetSection("Tables")
                                      .GetValue<string>("ItemQuantities");

            this.Table = this.TableStorageClient.GetTableReference(TABLE_NAME);
            this.Table.CreateIfNotExists();
        }

        public async Task<int?> GetQty(string itemId)   
        {
            if (itemId == null)
                return null;

            TableOperation retrieveOperation = TableOperation.Retrieve<ItemQuantityEntity>(
                                                      itemId, string.Empty, new List<string>(){"Qty"});

            TableResult result = await this.Table.ExecuteAsync(retrieveOperation);  

            if (result == null || result.Result == null)
                return null;

            ItemQuantityEntity retrievedEntity = result.Result as ItemQuantityEntity;  
            return retrievedEntity.Qty;
        }

        public async Task<Dictionary<string, int>> UpdateQuantities(
                            Dictionary<string, int> newItemsQuantities)
        {
            if (newItemsQuantities == null || !newItemsQuantities.Any())
                return null;

            IEnumerable<ItemQuantityEntity> newEntities =
                   newItemsQuantities.Select(pair =>
                             new ItemQuantityEntity(pair.Key, pair.Value)
                             {
                                ETag = "*"
                             });

            Dictionary<string, int> updatedQuantities = new Dictionary<string, int>();

            foreach(var entity in newEntities)
            {
                TableOperation operation = TableOperation.InsertOrMerge(entity);
                var result = await this.Table.ExecuteAsync(operation);

                if (result == null || result.Result == null)
                    continue;

                ItemQuantityEntity updatedOrInsertedEntity = result.Result as ItemQuantityEntity;
                updatedQuantities.Add(updatedOrInsertedEntity.PartitionKey, updatedOrInsertedEntity.Qty);
            }

            return updatedQuantities;
        }

        public async Task<int?> AddOrUpdateNewItemQty(string itemId, int itemQty)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return null;

            ItemQuantityEntity newEntity = new ItemQuantityEntity(itemId, itemQty)
            {
                ETag = "*"
            };

            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(newEntity);
            TableResult result = await this.Table.ExecuteAsync(insertOrReplaceOperation);

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;

            ItemQuantityEntity addedEntity = result.Result as ItemQuantityEntity;

            return addedEntity?.Qty;
        }
    }
}
