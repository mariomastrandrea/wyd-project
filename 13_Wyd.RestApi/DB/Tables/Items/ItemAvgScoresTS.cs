using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using _13_Wyd.RestApi.DB.Entities.Items;
using AzureTableStorage.TableQueryAsync;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Configuration;

namespace _13_Wyd.RestApi.DB.Tables.Items
{
    public class ItemAvgScoresTS
    {
        private readonly CloudTableClient TableStorageClient;
        private readonly CloudTable Table;
        private readonly string TABLE_NAME = "ItemScores";


        public ItemAvgScoresTS(CloudTableClient tableStorageClient, IConfiguration configuration)
        {
            this.TableStorageClient = tableStorageClient;
            TABLE_NAME = configuration.GetSection("TableStorage")
                                      .GetSection("Tables")
                                      .GetValue<string>("ItemAvgScores");

            this.Table = this.TableStorageClient.GetTableReference(TABLE_NAME);
            this.Table.CreateIfNotExists();
        }

        public async Task<double?> GetAvgScoreOf(string itemId)
        {
            if (itemId == null) return null;

            TableOperation retrieveOperation = TableOperation.Retrieve<ItemAvgScoreEntity>(
                itemId, string.Empty, new List<string>() {"AvgScore"});

            TableResult result = await this.Table.ExecuteAsync(retrieveOperation);

            if (result.HttpStatusCode >= 500) return null;

            if (result.HttpStatusCode == 404 || result.Result == null || result == null)
                return 0.0;

            ItemAvgScoreEntity retrievedEntity = result.Result as ItemAvgScoreEntity;
            return retrievedEntity.AvgScore;
        }

        public async Task<Dictionary<string,decimal>> FilterItemsByAvgScore(
                                        Expression<Func<ItemAvgScoreEntity,bool>> filter)
        {
            var query = this.Table.CreateQuery<ItemAvgScoreEntity>()
                                    .Where(filter)
                                    .AsTableQuery();

            IEnumerable<ItemAvgScoreEntity> filteredItemsScores = await query.ExecuteAsync();

            if (filteredItemsScores == null) return null;

            Dictionary<string, decimal> result = filteredItemsScores.ToDictionary(
                                pair => pair.PartitionKey, pair => (decimal)pair.AvgScore);

            return result;
        }

        public async Task<double?> UpdateAvgScoreOf(string itemId, double avgScore)
        {
            if (itemId == null || double.IsNaN(avgScore)) return null;

            ItemAvgScoreEntity newEntity = new ItemAvgScoreEntity(itemId, avgScore)
            {
                ETag = "*"
            };

            TableOperation updateOperation = TableOperation.Merge(newEntity);
            TableResult result = await this.Table.ExecuteAsync(updateOperation);

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;

            ItemAvgScoreEntity updatedEntity = result.Result as ItemAvgScoreEntity;
            return updatedEntity.AvgScore;
        }

        public async Task<double?> AddOrUpdateNewAvgScore(string itemId, double avgScore)
        {
            if (string.IsNullOrWhiteSpace(itemId) || double.IsNaN(avgScore))
                return null;

            ItemAvgScoreEntity newEntity = new ItemAvgScoreEntity(itemId, avgScore)
            {
                ETag = "*"
            };

            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(newEntity);

            TableResult result = await this.Table.ExecuteAsync(insertOrReplaceOperation);

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;

            ItemAvgScoreEntity addedEntity = result.Result as ItemAvgScoreEntity;

            return addedEntity.AvgScore;
        }
    }
}
