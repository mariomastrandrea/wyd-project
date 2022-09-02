using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.RestApi.DB.Entities.Items;
using AzureTableStorage.TableQueryAsync;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Configuration;

namespace _13_Wyd.RestApi.DB.Tables.Items
{
    public class ItemsTS    //Items DAO
    {
        private readonly CloudTableClient TableStorageClient;
        private readonly CloudTable Table;
        private readonly string TABLE_NAME;


        public ItemsTS(CloudTableClient tableStorageClient, IConfiguration configuration)
        {
            this.TableStorageClient = tableStorageClient;
            TABLE_NAME = configuration.GetSection("TableStorage")
                                      .GetSection("Tables")
                                      .GetValue<string>("Items");

            this.Table = this.TableStorageClient.GetTableReference(TABLE_NAME);
            this.Table.CreateIfNotExists();
        }

        public async Task<Item> GetItem(string itemId)
        {
            if (itemId == null || itemId.Length == 0)
                return null;

            var query = this.Table.CreateQuery<ItemEntity>()
                                                .Where(row => row.PartitionKey.Equals(itemId))
                                                .AsTableQuery();

            IEnumerable<ItemEntity> result = await query.ExecuteAsync();

            if (result == null || !result.Any())
                return null;

            if (result.Count() > 1)
                throw new Exception("Unexpectedly retrieved more than one Item");

            ItemEntity entityRetrieved = result.First();

            return entityRetrieved.ToItem();
        }

        public async Task<IEnumerable<Item>> GetItems()     //ok
        {
            var query = this.Table.CreateQuery<ItemEntity>();

            IEnumerable<ItemEntity> result = await query.ExecuteAsync();

            if (result == null)
                return null;

            IEnumerable<Item> allItems = result.Select(entity => entity.ToItem());
            return allItems;
        }

        public async Task<IEnumerable<Item>> GetItems(IEnumerable<string> itemIDs)
        {
            /* It doesn't work ('.Contains()' is not allowed)
             
            var query = this.Table.CreateQuery<ItemEntity>()
                                .Where(row => itemIDs.Contains(row.PartitionKey))
                                .AsTableQuery();

            IEnumerable<ItemEntity> result = await query.ExecuteAsync();

            if (result == null) return null;

            IEnumerable<Item> searchedItems = result.Select(entity => entity.ToItem());
            return searchedItems;
            */

            List<Item> searchedItems = new List<Item>();

            foreach(string itemId in itemIDs)
            {
                Item item = await this.GetItem(itemId);

                if (item == null) return null;  //an error occurred retrieving an item -> quit

                searchedItems.Add(item); 
            }

            return searchedItems;
        }

        public async Task<IEnumerable<Item>> SearchItems(Expression<Func<ItemEntity, bool>> filter)
        {
            var query = this.Table.CreateQuery<ItemEntity>()
                                            .Where(filter).AsTableQuery();


            IEnumerable<ItemEntity> result = await query.ExecuteAsync();

            if (result == null)
                return null;

            IEnumerable<Item> searchedItems = result.Select(entity => entity.ToItem());
            return searchedItems;
        }

        public async Task<Item> Create(Item newItem)    //ok
        {
            if (newItem == null || string.IsNullOrWhiteSpace(newItem.Id))
                return null;

            ItemEntity newEntity = new ItemEntity(newItem);
            TableOperation insertOperation = TableOperation.Insert(newEntity);

            TableResult result = await this.Table.ExecuteAsync(insertOperation);

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;

            ItemEntity createdEntity = (ItemEntity)result.Result;   
            return createdEntity.ToItem();                         
        }

        public async Task<Item> Delete(string itemId)       //ok
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return null;

            Item itemToDelete = await this.GetItem(itemId);

            if (itemToDelete == null)
                return null;  //no items with ID = {itemId}

            ItemEntity entityToDelete = new ItemEntity(itemToDelete)
            {
                ETag = "*"
            };

            TableOperation deleteOperation = TableOperation.Delete(entityToDelete);
            TableResult result = await this.Table.ExecuteAsync(deleteOperation);

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;    //an error occured

            Item deletedItem = (result.Result as ItemEntity).ToItem();
            return deletedItem;
        }

        public async Task<Item> Update(Item newItem)   
        {
            if (newItem == null || string.IsNullOrWhiteSpace(newItem.Id))
                return null;

            Item itemToUpdate = await this.GetItem(newItem.Id);

            if (itemToUpdate == null) return null;

            ItemEntity entityToMerge = new ItemEntity(newItem)
            {
                ETag = "*"
            };

            TableOperation mergeOperation = TableOperation.Merge(entityToMerge);
            TableResult result = await this.Table.ExecuteAsync(mergeOperation);   

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;

            ItemEntity updatedEntity = result.Result as ItemEntity;                 
            Item updatedItem = updatedEntity.ToItem().FillWith(itemToUpdate);

            return updatedItem;
        }

        public async Task<IEnumerable<Item>> UpdateAll(IEnumerable<Item> newItems)  
        {
            if (newItems == null || !newItems.Any())
                return null;

            List<Item> updatedItems = new List<Item>();

            foreach(Item itemToUpdate in newItems)
            {
                Item retrievedItem = await this.GetItem(itemToUpdate.Id);

                if (retrievedItem == null)    //ignore not found items 
                    continue;

                ItemEntity entityToUpdate = new ItemEntity(itemToUpdate)
                {
                    ETag = "*"
                };

                TableResult result =
                    await this.Table.ExecuteAsync(TableOperation.Merge(entityToUpdate));

                if (result == null || result.Result == null
                    || result.HttpStatusCode >= 400)   //an error occured
                    continue;

                ItemEntity updatedEntity = result.Result as ItemEntity;
                Item updatedItem = updatedEntity.ToItem().FillWith(retrievedItem);

                updatedItems.Add(updatedItem);
            }

            return updatedItems;
        }
    }
}
