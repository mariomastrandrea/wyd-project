using System;
using Microsoft.Azure.Cosmos.Table;

namespace _13_Wyd.RestApi.DB.Entities.Users
{
    public class UserItemEntity : TableEntity
    {
        public int Qty { get; set; }

        public UserItemEntity() { }

        public UserItemEntity(string userId, string itemId, int qty)
        {
            this.PartitionKey = userId;
            this.RowKey = itemId;

            this.Qty = qty;
        }
    }
}
