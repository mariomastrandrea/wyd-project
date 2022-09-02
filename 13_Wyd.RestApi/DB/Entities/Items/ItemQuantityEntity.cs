using System;
using Microsoft.Azure.Cosmos.Table;

namespace _13_Wyd.RestApi.DB.Entities.Items
{
    public class ItemQuantityEntity : TableEntity
    {
        public int Qty { get; set; }

        public ItemQuantityEntity() { }

        public ItemQuantityEntity(string itemId, int qty)
        {
            this.PartitionKey = itemId;
            this.RowKey = string.Empty;

            this.Qty = qty;
        }
    }
}
