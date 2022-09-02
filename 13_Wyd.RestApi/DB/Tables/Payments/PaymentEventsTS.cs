using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.ModelClasses.Payment.Events;
using _13_Wyd.RestApi.DB.Entities.Payments;
using AzureTableStorage.TableQueryAsync;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Configuration;

namespace _13_Wyd.RestApi.DB.Tables.Payments
{
    public class PaymentEventsTS
    {
        private readonly CloudTableClient TableStorageClient;
        private readonly CloudTable Table;
        private readonly string TABLE_NAME;


        public PaymentEventsTS(CloudTableClient tableStorageClient, IConfiguration configuration)
        {
            this.TableStorageClient = tableStorageClient;
            TABLE_NAME = configuration.GetSection("TableStorage")
                                      .GetSection("Tables")
                                      .GetValue<string>("PaymentEvents");

            this.Table = this.TableStorageClient.GetTableReference(TABLE_NAME);
            this.Table.CreateIfNotExists();
        }

        public async Task<PaymentEvent> AddNewPaymentEvent(PaymentEvent newEvent)
        {
            if (newEvent == null || string.IsNullOrWhiteSpace(newEvent.PaymentId) ||
                string.IsNullOrWhiteSpace(newEvent.OrderId) || string.IsNullOrWhiteSpace(newEvent.UserId) ||
                !newEvent.ItemsByQty.Any())
                return null;

            PaymentEventEntity newEntity = new PaymentEventEntity(newEvent);

            TableOperation insertOperation = TableOperation.Insert(newEntity);
            TableResult result = await this.Table.ExecuteAsync(insertOperation);

            if(result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;

            PaymentEventEntity insertedEntity = result.Result as PaymentEventEntity;
            return insertedEntity.ToPaymentEvent(newEvent.ItemsByQty);
        }

        public async Task<PaymentEvent> GetPaymentEvent(string paymentId, string orderId,
            Func<string, Task<Item>> itemTransformation)
        {
            if (string.IsNullOrWhiteSpace(paymentId) || string.IsNullOrWhiteSpace(orderId)
                || itemTransformation == null)
                return null;

            var query = this.Table.CreateQuery<PaymentEventEntity>()
                                    .Where(row => row.PartitionKey.Equals(paymentId) &&
                                                    row.RowKey.Equals(orderId))
                                    .AsTableQuery();

            IEnumerable<PaymentEventEntity> result = await query.ExecuteAsync();

            if (result == null || !result.Any())
                return null;

            if (result.Count() > 1)
                throw new Exception("Unexpectedly retrieved more than one payment");

            PaymentEventEntity retrievedEntity = result.First();
            Dictionary<string, int> itemsIDsByQty = retrievedEntity.GetItemsIDsByQty();

            Dictionary<Item, int> itemsByQty = new Dictionary<Item, int>();
            foreach(var pair in itemsIDsByQty)
            {
                string itemId = pair.Key;
                int qty = pair.Value;

                Item item = await itemTransformation.Invoke(itemId);

                if (item == null) return null;

                itemsByQty.Add(item, qty);
            }

            return retrievedEntity.ToPaymentEvent(itemsByQty);
        }

        public async Task<PaymentEvent> GetPaymentEvents(Dictionary<string,string> paymentIdsByOrderId,
            Func<string, Task<Item>> itemTransformation)
        {
            if (paymentIdsByOrderId == null || !paymentIdsByOrderId.Any() || itemTransformation == null)
                return null;

            var query = this.Table.CreateQuery<PaymentEventEntity>()
                                  .Where(row => paymentIdsByOrderId.ContainsKey(row.PartitionKey)
                                     && paymentIdsByOrderId[row.PartitionKey].Equals(row.RowKey))                                           
                                  .AsTableQuery();

            IEnumerable<PaymentEventEntity> result = await query.ExecuteAsync();

            if (result == null || !result.Any())
                return null;

            if (result.Count() > 1)
                throw new Exception("Unexpectedly retrieved more than one payment");

            PaymentEventEntity retrievedEntity = result.First();

            Dictionary<string, int> itemsIDsByQty = retrievedEntity.GetItemsIDsByQty();
            Dictionary<Item, int> itemsByQty = new Dictionary<Item, int>();

            foreach(var pair in itemsIDsByQty)
            {
                string itemId = pair.Key;
                int qty = pair.Value;

                Item item = await itemTransformation.Invoke(itemId);

                if (item == null) return null;

                itemsByQty.Add(item, qty);
            }

            return retrievedEntity.ToPaymentEvent(itemsByQty);
        }

        public async Task<IEnumerable<PaymentEvent>> GetOrderPaymentEvents(string orderId,
            Func<string, Task<Item>> itemTransformation)
        {
            if (string.IsNullOrWhiteSpace(orderId) || itemTransformation == null)
                return null;

            var query = this.Table.CreateQuery<PaymentEventEntity>()
                                    .Where(row => row.RowKey.Equals(orderId))
                                    .AsTableQuery();

            IEnumerable<PaymentEventEntity> result = await query.ExecuteAsync();

            if (result == null || !result.Any())
                return null;

            List<PaymentEvent> orderPayments = new List<PaymentEvent>();
            foreach(var entity in result)
            {
                Dictionary<string, int> itemsIDsByQty = entity.GetItemsIDsByQty();
                Dictionary<Item, int> itemsByQty = new Dictionary<Item, int>();

                foreach(var pair in itemsIDsByQty)
                {
                    string itemId = pair.Key;
                    int qty = pair.Value;

                    Item item = await itemTransformation.Invoke(itemId);

                    if (item == null) return null;

                    itemsByQty.Add(item, qty);
                }

                orderPayments.Add(entity.ToPaymentEvent(itemsByQty));
            }

            return orderPayments;
        }

        public async Task<PaymentEvent> DeletePaymentEvent(string paymentId, string orderId,
            Func<string, Task<Item>> itemTransformation)
        {
            if (string.IsNullOrWhiteSpace(paymentId) || string.IsNullOrWhiteSpace(orderId) ||
                itemTransformation == null)
                return null;

            PaymentEvent paymentEventToDelete = 
                await this.GetPaymentEvent(paymentId, orderId, itemTransformation);

            if (paymentEventToDelete == null)   //there is no PaymentEvent with this IDs
                return null;

            PaymentEventEntity entityToDelete = new PaymentEventEntity(paymentEventToDelete)
            {
                ETag = "*"
            };

            TableOperation deleteOperation = TableOperation.Delete(entityToDelete);
            TableResult result = await this.Table.ExecuteAsync(deleteOperation);

            if (result == null || result.HttpStatusCode >= 400) return null;

            return paymentEventToDelete;
        }

        public async Task<PaymentEvent> SetProcessedPayment(PaymentEvent processedPayment)
        {
            if (processedPayment == null || string.IsNullOrWhiteSpace(processedPayment.PaymentId)
                || string.IsNullOrWhiteSpace(processedPayment.OrderId) || !processedPayment.Processed)
                return null;

            TableOperation retrieveOperation = TableOperation.Retrieve(
                processedPayment.PaymentId, processedPayment.OrderId);
            TableResult result = await this.Table.ExecuteAsync(retrieveOperation);

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;    //no such Payment in DB

            PaymentEventEntity entityToMerge = new PaymentEventEntity()
            {
                PartitionKey = processedPayment.PaymentId,
                RowKey = processedPayment.OrderId,
                Processed = true,
                ETag = "*"
            };

            TableOperation mergeOperation = TableOperation.Merge(entityToMerge);
            TableResult mergeResult = await this.Table.ExecuteAsync(mergeOperation);

            if (mergeResult == null || mergeResult.HttpStatusCode >= 400)
                return null; //some error occured

            return processedPayment;
        }
    }
}
