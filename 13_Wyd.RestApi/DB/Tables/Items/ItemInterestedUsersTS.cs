using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.RestApi.DB.Entities.Items;
using AzureTableStorage.TableQueryAsync;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Configuration;

namespace _13_Wyd.RestApi.DB.Tables.Items
{
    public class ItemInterestedUsersTS
    {
        private readonly CloudTableClient TableStorageClient;
        private readonly CloudTable Table;
        private readonly string TABLE_NAME;


        public ItemInterestedUsersTS(CloudTableClient tableStorageClient, IConfiguration configuration)
        {
            this.TableStorageClient = tableStorageClient;
            TABLE_NAME = configuration.GetSection("TableStorage")
                                      .GetSection("Tables")
                                      .GetValue<string>("ItemInterestedUsers");

            this.Table = this.TableStorageClient.GetTableReference(TABLE_NAME);
            this.Table.CreateIfNotExists();
        }

        public async Task<IEnumerable<string>> GetInterestedUsersIDs(string itemId)
        {
            if (itemId == null)
                return null;

            var query = this.Table.CreateQuery<ItemInterestedUserEntity>()
                                        .Where(row => row.PartitionKey.Equals(itemId))
                                        .AsTableQuery();

            IEnumerable<ItemInterestedUserEntity> interestedUsersEntities =
                                        await query.ExecuteAsync();

            if (interestedUsersEntities == null)
                return null;

            IEnumerable<string> interestedUsersIDs =
                interestedUsersEntities.Select(entity => entity.RowKey);

            return interestedUsersIDs;
        }

        public async Task<IEnumerable<string>> Update(string itemId,
                                    IEnumerable<string> interestedUsersIDs)
        {
            if (itemId == null || interestedUsersIDs == null)
                return null;

            IEnumerable<string> oldInterestedUsersIDs = await this.GetInterestedUsersIDs(itemId);

            IEnumerable<string> usersToDelete = oldInterestedUsersIDs.Except(interestedUsersIDs);
            IEnumerable<string> deletedUsersIDs = Enumerable.Empty<string>();

            if(usersToDelete.Any())
            {
                TableBatchOperation deleteBatch = new TableBatchOperation();

                IEnumerable<ItemInterestedUserEntity> entitiesToDelete =
                           usersToDelete.Select(userId => new ItemInterestedUserEntity(itemId, userId)
                           {
                               ETag = "*"
                           });

                IEnumerable<TableOperation> deleteOperations =
                    entitiesToDelete.Select(entity => TableOperation.Delete(entity));

                foreach (var operation in deleteOperations)
                    deleteBatch.Add(operation);

                var result = await this.Table.ExecuteBatchAsync(deleteBatch);

                if (result == null) return null;

                IEnumerable<ItemInterestedUserEntity> deletedEntities =
                    result.Select(tResult => tResult.Result as ItemInterestedUserEntity);

                deletedUsersIDs = deletedEntities.Select(entity => entity.RowKey);
            }


            IEnumerable<string> usersToInsert = interestedUsersIDs.Except(oldInterestedUsersIDs);
            IEnumerable<string> insertedUsersIDs = Enumerable.Empty<string>();

            if(usersToInsert.Any())
            {
                TableBatchOperation insertBatch = new TableBatchOperation();

                IEnumerable<ItemInterestedUserEntity> entitiesToInsert =
                           usersToInsert.Select(userId => new ItemInterestedUserEntity(itemId, userId)
                           {
                               ETag = "*"
                           });

                IEnumerable<TableOperation> insertOperations =
                    entitiesToInsert.Select(entity => TableOperation.Insert(entity));

                foreach (var operation in insertOperations)
                    insertBatch.Add(operation);

                var result = await this.Table.ExecuteBatchAsync(insertBatch);

                if (result == null) return null;

                IEnumerable<ItemInterestedUserEntity> insertedEntities =
                    result.Select(tResult => (ItemInterestedUserEntity)tResult.Result);

                insertedUsersIDs = insertedEntities.Select(entity => entity.RowKey);
            }

            IEnumerable<string> updatedUsersIDs =
                oldInterestedUsersIDs.Except(deletedUsersIDs).Concat(insertedUsersIDs);
   
            return updatedUsersIDs;
        }
    }
}
