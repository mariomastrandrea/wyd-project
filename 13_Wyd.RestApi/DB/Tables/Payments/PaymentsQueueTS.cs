using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _13_Wyd.RestApi.DB.Entities.Payments;
using AzureTableStorage.TableQueryAsync;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;

namespace _13_Wyd.RestApi.DB.Tables.Payments
{
    public class PaymentsQueueTS
    {
        private readonly CloudTableClient TableStorageClient;
        private readonly CloudTable Table;
        private readonly string TABLE_NAME;


        public PaymentsQueueTS(CloudTableClient tableStorageClient, IConfiguration configuration)
        {
            this.TableStorageClient = tableStorageClient;
            TABLE_NAME = configuration.GetSection("TableStorage")
                                      .GetSection("Tables")
                                      .GetValue<string>("PaymentsQueue");

            this.Table = this.TableStorageClient.GetTableReference(TABLE_NAME);
            this.Table.CreateIfNotExists();
        }

        public async Task<Tuple<string,string>> EnqueueNewPayment(string paymentId, string orderId)
        {
            if (string.IsNullOrWhiteSpace(paymentId) || string.IsNullOrWhiteSpace(orderId))
                return null;

            QueuePaymentEntity newEntity = new QueuePaymentEntity(paymentId, orderId);

            TableOperation insertOperation = TableOperation.Insert(newEntity);
            TableResult result = await this.Table.ExecuteAsync(insertOperation);

            if (result == null || result.HttpStatusCode >= 400)
                return null;

            return new Tuple<string,string>(paymentId, orderId);
        }

        public async Task<Tuple<string,string>> GetPaymentIds(string paymentId, string orderId)
        {
            if (string.IsNullOrWhiteSpace(paymentId) || string.IsNullOrWhiteSpace(orderId))
                return null;

            TableOperation retrieveOperation =
                TableOperation.Retrieve<QueuePaymentEntity>(paymentId, orderId, new List<string>());

            TableResult result = await this.Table.ExecuteAsync(retrieveOperation);

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;

            QueuePaymentEntity retrievedEntity = result.Result as QueuePaymentEntity;
            return retrievedEntity.ToTuple();
        }

        public async Task<IEnumerable<Tuple<string,string>>> GetQueue()
        {
            var query = this.Table.CreateQuery<QueuePaymentEntity>();
                                  
            IEnumerable<QueuePaymentEntity> queueEntities = await query.ExecuteAsync();

            if (queueEntities == null) return null;

            IEnumerable<Tuple<string, string>> queueTuples =
                queueEntities.Select(entity => entity.ToTuple());

            return queueTuples;
        }

        public async Task<Tuple<string,string>> DequeuePayment(string paymentId, string orderId)
        {
            if (string.IsNullOrWhiteSpace(paymentId) || string.IsNullOrWhiteSpace(orderId))
                return null;

            Tuple<string, string> paymentToDelete = await this.GetPaymentIds(paymentId, orderId);

            if (paymentToDelete == null)   //there is no QueuePayment with these IDs
                return null;

            QueuePaymentEntity entityToDelete = new QueuePaymentEntity(paymentId, orderId)
            {
                ETag = "*"
            };

            TableOperation deleteOperation = TableOperation.Delete(entityToDelete);
            TableResult result = await this.Table.ExecuteAsync(deleteOperation);

            if (result == null || result.HttpStatusCode >= 400) return null;

            return paymentToDelete;
        }
    }
}
