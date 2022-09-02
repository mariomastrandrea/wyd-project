using System;
using Microsoft.Azure.Cosmos.Table;

namespace _13_Wyd.RestApi.DB.Entities.Payments
{
    public class QueuePaymentEntity : TableEntity
    {
        public QueuePaymentEntity() { }

        public QueuePaymentEntity(string paymentId, string orderId)
        {
            this.PartitionKey = paymentId;
            this.RowKey = orderId;
        }

        public Tuple<string, string> ToTuple()
        {
            return new Tuple<string, string>(this.PartitionKey, this.RowKey);
        }
    }
}
