using System;
using Microsoft.Azure.Cosmos.Table;

namespace _13_Wyd.RestApi.DB.Entities.Items
{
    public class ItemAvgScoreEntity : TableEntity
    {
        public double AvgScore { get; set; }    // converted: decimal <-> double 

        public ItemAvgScoreEntity() { }

        public ItemAvgScoreEntity(string itemId, double avgScore)
        {
            this.PartitionKey = itemId;
            this.RowKey = string.Empty;

            this.AvgScore = avgScore;
        }
    }
}
