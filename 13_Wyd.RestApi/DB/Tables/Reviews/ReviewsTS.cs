using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.RestApi.DB.Entities.Reviews;
using AzureTableStorage.TableQueryAsync;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Configuration;

namespace _13_Wyd.RestApi.DB.Tables.Reviews
{
    public class ReviewsTS
    {
        private readonly CloudTableClient TableStorageClient;
        private readonly CloudTable Table;
        private readonly string TABLE_NAME;


        public ReviewsTS(CloudTableClient tableStorageClient, IConfiguration configuration)
        {
            this.TableStorageClient = tableStorageClient;
            TABLE_NAME = configuration.GetSection("TableStorage")
                                      .GetSection("Tables")
                                      .GetValue<string>("Reviews");

            this.Table = this.TableStorageClient.GetTableReference(TABLE_NAME);
            this.Table.CreateIfNotExists();
        }

        public async Task<IEnumerable<Review>> GetItemReviews(string itemId)
        {
            if (itemId == null) return null;

            var query = this.Table.CreateQuery<ReviewEntity>()
                                    .Where(row => row.PartitionKey.Equals(itemId))
                                    .AsTableQuery();

            IEnumerable<ReviewEntity> queryResult = await query.ExecuteAsync();

            if (queryResult == null) return null;

            IEnumerable<Review> itemReviews = queryResult.Select(entity => entity.ToReview());
            return itemReviews;
        }

        public async Task<IEnumerable<Review>> GetUserReviews(string userId)
        {
            if (userId == null) return null;

            var query = this.Table.CreateQuery<ReviewEntity>()
                                    .Where(row => row.RowKey.Equals(userId))
                                    .AsTableQuery();

            IEnumerable<ReviewEntity> queryResult = await query.ExecuteAsync();

            if (queryResult == null) return null;

            IEnumerable<Review> userReviews = queryResult.Select(entity => entity.ToReview());
            return userReviews;
        }

        public async Task<IEnumerable<Review>> UpdateItemReviews(string itemId,
                                                    IEnumerable<Review> newReviews)
        {
            if (itemId == null || newReviews == null)
                return null;

            IEnumerable<Review> oldItemReviews = await this.GetItemReviews(itemId);

            return await this.UpdateReviews(oldItemReviews, newReviews);
        }

        public async Task<IEnumerable<Review>> UpdateUserReviews(string userId,
                                                    IEnumerable<Review> newReviews)
        {
            if (userId == null || newReviews == null)
                return null;

            IEnumerable<Review> oldUserReviews = await this.GetUserReviews(userId);

            return await this.UpdateReviews(oldUserReviews, newReviews);
        }

        private async Task<IEnumerable<Review>> UpdateReviews(IEnumerable<Review> oldReviews,
            IEnumerable<Review> newReviews)
        {
            if (oldReviews == null || newReviews == null)
                return null;

            IEnumerable<Review> reviewsToDelete = oldReviews.Except(newReviews);
            IEnumerable<Review> deletedReviews = Enumerable.Empty<Review>();

            if (reviewsToDelete.Any())
            {
                TableBatchOperation deleteBatch = new TableBatchOperation();

                IEnumerable<ReviewEntity> entitiesToDelete =
                    reviewsToDelete.Select(review => new ReviewEntity(review)
                    {
                        ETag = "*"
                    });

                foreach (var entity in entitiesToDelete)
                {
                    TableOperation deleteOperation = TableOperation.Delete(entity);
                    deleteBatch.Add(deleteOperation);
                }

                var result = await this.Table.ExecuteBatchAsync(deleteBatch);

                if (result == null) return null;

                IEnumerable<ReviewEntity> deletedEntities =
                    result.Select(tResult => tResult.Result as ReviewEntity);

                deletedReviews = deletedEntities.Select(entity => entity?.ToReview())
                                                .Where(r => r != null);
            }

            IEnumerable<Review> reviewsToAdd = newReviews.Except(oldReviews);
            IEnumerable<Review> addedReviews = Enumerable.Empty<Review>();

            if (reviewsToAdd.Any())
            {
                TableBatchOperation insertBatch = new TableBatchOperation();

                IEnumerable<ReviewEntity> entitiesToInsert =
                    reviewsToAdd.Select(review => new ReviewEntity(review)
                    {
                        ETag = "*"
                    });

                foreach (var entity in entitiesToInsert)
                {
                    TableOperation insertOperation = TableOperation.Insert(entity);
                    insertBatch.Add(insertOperation);
                }

                var result = await this.Table.ExecuteBatchAsync(insertBatch);

                if (result == null) return null;

                IEnumerable<ReviewEntity> insertedEntities =
                    result.Select(tResult => tResult.Result as ReviewEntity);

                addedReviews = insertedEntities.Select(entity => entity?.ToReview())
                                                .Where(r => r != null);
            }

            IEnumerable<Review> reviewsToUpdate = oldReviews.Except(deletedReviews);
            IEnumerable<Review> updatedReviews = Enumerable.Empty<Review>();

            if(reviewsToUpdate.Any())
            {
                TableBatchOperation mergeBatch = new TableBatchOperation();

                IEnumerable<ReviewEntity> entitiesToMerge =
                        reviewsToUpdate.Select(review => new ReviewEntity(review)
                        {
                            ETag = "*"
                        });

                foreach(var entity in entitiesToMerge)
                {
                    TableOperation mergeOperation = TableOperation.Merge(entity);
                    mergeBatch.Add(mergeOperation);
                }

                var result = await this.Table.ExecuteBatchAsync(mergeBatch);

                if (result == null) return null;

                IEnumerable<ReviewEntity> mergedEntities =
                    result.Select(tResult => tResult.Result as ReviewEntity);

                updatedReviews = mergedEntities.Where(entity => entity != null)
                                               .Select(entity => entity.ToReview());
            }

            IEnumerable<Review> newUpdatedReviews = updatedReviews.Concat(addedReviews);

            return newUpdatedReviews;
        }
    }
}
