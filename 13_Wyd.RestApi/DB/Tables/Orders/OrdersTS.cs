using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.RestApi.DB.Entities.Orders;
using AzureTableStorage.TableQueryAsync;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Configuration;

namespace _13_Wyd.RestApi.DB.Tables.Orders
{
    public class OrdersTS
    {
        private readonly CloudTableClient TableStorageClient;
        private readonly CloudTable Table;
        private readonly string TABLE_NAME;


        public OrdersTS(CloudTableClient tableStorageClient, IConfiguration configuration)
        {
            this.TableStorageClient = tableStorageClient;
            this.TABLE_NAME = configuration.GetSection("TableStorage")
                                           .GetSection("Tables")
                                           .GetValue<string>("Orders");

            this.Table = this.TableStorageClient.GetTableReference(TABLE_NAME);
            this.Table.CreateIfNotExists();
        }

        public async Task<Order> GetOrder(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return null;

            var query = this.Table.CreateQuery<OrderEntity>()
                                  .Where(row => row.RowKey.Equals(orderId))
                                  .AsTableQuery();

            IEnumerable<OrderEntity> result = await query.ExecuteAsync();

            if (result == null) return null;

            if (result.Count() > 1)
                throw new Exception("Unexpectedly retrieved more than one entity");

            OrderEntity entity = result.First();

            return entity.ToOrder();
        }

        public async Task<Order> GetOrder(string orderId, string userId)
        {
            if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(userId))
                return null;

            var query = this.Table.CreateQuery<OrderEntity>()       //***
                                  .Where(row => row.PartitionKey.Equals(userId) &&
                                                row.RowKey.Equals(orderId))
                                  .AsTableQuery();

            IEnumerable<OrderEntity> result = await query.ExecuteAsync();

            if (result == null) return null;

            if (result.Count() > 1)
                throw new Exception("Unexpectedly retrieved more than one entity");

            OrderEntity entity = result.First();

            return entity.ToOrder();
        }

        public async Task<Order> GetUserOrder(string userId, string orderId)
        {
            return await this.GetOrder(orderId, userId);
        }

        public async Task<IEnumerable<Order>> GetOrders(IEnumerable<string> ordersIDs, string userId)
        {
            if (ordersIDs == null || string.IsNullOrWhiteSpace(userId) || ordersIDs == null)
                return null;

            /*
            var query = this.Table.CreateQuery<OrderEntity>()
                                  .Where(row => row.PartitionKey.Equals(userId) &&
                                                ordersIDs.Contains(row.RowKey)) //.Contains() does not work
                                  .AsTableQuery();

            IEnumerable<OrderEntity> ordersEntities = await query.ExecuteAsync();

            if (ordersEntities == null || !ordersEntities.Any()) return null;

            IEnumerable<Order> orders = ordersEntities.Select(entity => entity.ToOrder());
            return orders;
            */

            IEnumerable<Order> userOrders = await this.GetUserOrders(userId);

            if (userOrders == null || !userOrders.Any())
                return userOrders;

            return userOrders.Where(order => ordersIDs.Contains(order.Id));
        }

        public async Task<IEnumerable<Order>> GetOrders()
        {
            var query = this.Table.CreateQuery<OrderEntity>();
                                 
            IEnumerable<OrderEntity> allOrdersEntities = await query.ExecuteAsync();

            if (allOrdersEntities == null) return null;

            IEnumerable<Order> allOrders = allOrdersEntities.Select(entity => entity.ToOrder());
            return allOrders;
        }

        //*** : better to do this, than TableOperation.Retrieve(),
        //      because that one requires a list of column names

        public async Task<IEnumerable<Order>> GetUserOrders(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var query = this.Table.CreateQuery<OrderEntity>()
                                  .Where(row => row.PartitionKey.Equals(userId))
                                  .AsTableQuery();

            IEnumerable<OrderEntity> ordersEntities = await query.ExecuteAsync();

            if (ordersEntities == null) return null;

            IEnumerable<Order> orders = ordersEntities.Select(entity => entity.ToOrder());
            return orders;
        }

        public async Task<Order> Create(Order newOrder)
        {
            if (newOrder == null || string.IsNullOrWhiteSpace(newOrder.Id)) return null;

            OrderEntity newEntity = new OrderEntity(newOrder);

            TableOperation insertOperation = TableOperation.Insert(newEntity);
            TableResult result = await this.Table.ExecuteAsync(insertOperation);

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;

            OrderEntity createdEntity = result.Result as OrderEntity;
            return createdEntity.ToOrder();
        }

        public async Task<Order> Update(Order updatedOrder)
        {
            if (updatedOrder == null || string.IsNullOrWhiteSpace(updatedOrder.Id))
                return null;

            OrderEntity entityToMerge = new OrderEntity(updatedOrder)
            {
                ETag = "*"
            };

            TableOperation mergeOperation = TableOperation.Merge(entityToMerge);
            TableResult result = await this.Table.ExecuteAsync(mergeOperation);

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;

            OrderEntity mergedEntity = result.Result as OrderEntity;
            return mergedEntity.ToOrder();  //* this order must be filled...
        }

        public async Task<Order> Delete(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return null;

            Order orderToDelete = await this.GetOrder(orderId);

            if (orderToDelete == null) return null;

            OrderEntity entityToDelete = new OrderEntity(orderToDelete)
            {
                ETag = "*"
            };

            TableOperation deleteOperation = TableOperation.Delete(entityToDelete);
            TableResult result = await this.Table.ExecuteAsync(deleteOperation);

            if (result == null || result.Result == null || result.HttpStatusCode >= 400)
                return null;

            OrderEntity deletedEntity = result.Result as OrderEntity;
            return deletedEntity.ToOrder();
        }
    }
}
