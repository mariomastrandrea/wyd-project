using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _13_Wyd.RestApi.DB.Entities.Users;
using Microsoft.Azure.Cosmos.Table;

namespace _13_Wyd.RestApi.DB.Tables.Users
{
    public static class UserItemsExtensions
    {
        public static async Task<Dictionary<string, int>> Update(CloudTable table, string userId,
            Dictionary<string, int> oldItemsIDs, Dictionary<string, int> newItemsIDs)
        {
            if (oldItemsIDs == null || newItemsIDs == null || table == null
                || string.IsNullOrWhiteSpace(userId))
                return null;

            IEnumerable<string> itemsToDelete = oldItemsIDs.Keys.Except(newItemsIDs.Keys);
            Dictionary<string, int> deletedItems = new Dictionary<string, int>();

            if (itemsToDelete.Any())
            {
                TableBatchOperation deleteBatch = new TableBatchOperation();

                IEnumerable<UserItemEntity> entitiesToDelete =
                    itemsToDelete.Select(itemId =>
                    {
                        return new UserItemEntity(userId, itemId, oldItemsIDs[itemId])
                        {
                            ETag = "*"
                        };
                    });

                foreach (var entity in entitiesToDelete)
                {
                    TableOperation operation = TableOperation.Delete(entity);
                    deleteBatch.Add(operation);
                }

                var results = await table.ExecuteBatchAsync(deleteBatch);

                if (results == null) return null;

                IEnumerable<UserItemEntity> deletedEntities =
                    results.Select(result => result.Result as UserItemEntity);  

                deletedItems = deletedEntities.ToDictionary(entity => entity.RowKey,
                                                            entity => entity.Qty);
            }

            IEnumerable<string> itemsToAdd = newItemsIDs.Keys.Except(oldItemsIDs.Keys);
            Dictionary<string, int> addedItems = new Dictionary<string, int>();

            if (itemsToAdd.Any())
            {
                TableBatchOperation insertBatch = new TableBatchOperation();

                IEnumerable<UserItemEntity> entitiesToInsert =
                    itemsToAdd.Select(itemId =>
                    {
                        return new UserItemEntity(userId, itemId, newItemsIDs[itemId]);
                    });

                foreach (var newEntity in entitiesToInsert)
                {
                    TableOperation operation = TableOperation.Insert(newEntity);
                    insertBatch.Add(operation);
                }

                var results = await table.ExecuteBatchAsync(insertBatch);

                if (results == null) return null;

                IEnumerable<UserItemEntity> insertedEntities =
                    results.Select(result => result.Result as UserItemEntity);  

                addedItems = insertedEntities.ToDictionary(entity => entity.RowKey,
                                                           entity => entity.Qty);
            }

            IEnumerable<string> itemsToUpdate = oldItemsIDs.Keys.Except(deletedItems.Keys);
            Dictionary<string, int> updatedItems = new Dictionary<string, int>();

            if (itemsToUpdate.Any())
            {
                TableBatchOperation mergeBatch = new TableBatchOperation();

                IEnumerable<UserItemEntity> entitiesToMerge =
                    itemsToUpdate.Select(itemId =>
                    {
                        return new UserItemEntity(userId, itemId, newItemsIDs[itemId])
                        {
                            ETag = "*"
                        };
                    });

                foreach (var newEntity in entitiesToMerge)
                {
                    TableOperation operation = TableOperation.Merge(newEntity);
                    mergeBatch.Add(operation);
                }

                var results = await table.ExecuteBatchAsync(mergeBatch);

                if (results == null) return null;

                IEnumerable<UserItemEntity> updatedEntities =
                    results.Select(result => result.Result as UserItemEntity); 

                updatedItems = updatedEntities.ToDictionary(entity => entity.RowKey,
                                                            entity => entity.Qty);
            }

            Dictionary<string, int> updatedShoppingCart =
                    updatedItems.Concat(addedItems).ToDictionary(pair => pair.Key, pair => pair.Value); 

            /*
            Dictionary<string, int> updatedShoppingCart =
                    oldItemsIDs.Except(deletedItems).Concat(addedItems)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);    
            */

            return updatedShoppingCart;
        }
    }
}
