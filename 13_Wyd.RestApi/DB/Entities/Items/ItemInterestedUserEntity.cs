using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace _13_Wyd.RestApi.DB.Entities.Items
{
    public class ItemInterestedUserEntity : TableEntity
    {
        public ItemInterestedUserEntity() { }

        public ItemInterestedUserEntity(string itemId, string userId)
        {
            this.PartitionKey = itemId;
            this.RowKey = userId;
        }
    }
}
