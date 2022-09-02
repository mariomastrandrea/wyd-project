using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.RestApi.DB.Tables.Orders;

namespace _13_Wyd.RestApi.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly OrdersTS OrdersTable;


        public OrdersRepository(OrdersTS ordersTable)
        {
            this.OrdersTable = ordersTable;
        }

        public async Task<Order> GetOrder(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return null;

            Order order = await this.OrdersTable.GetOrder(orderId);
            return order;
        }

        public async Task<IEnumerable<Order>> GetOrders()
        {
            IEnumerable<Order> allOrders = await this.OrdersTable.GetOrders();
            return allOrders;
        }

        public async Task<Order> CreateOrder(Order newOrder)
        {
            if (newOrder == null || !string.IsNullOrWhiteSpace(newOrder.Id))
                return null;

            string newGuid = Guid.NewGuid().ToString();  //setting guid as orderId
            newOrder.Id = $"OR-{newGuid}";     

            Order createdOrder = await this.OrdersTable.Create(newOrder);
            return createdOrder;
        }

        public async Task<Order> DeleteOrder(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return null;

            Order deletedOrder = await this.OrdersTable.Delete(orderId);
            return deletedOrder;
        }
        
        public async Task<Order> UpdateOrder(Order updatedOrder)
        {
            if (updatedOrder == null || updatedOrder.Id == null)
                return null;

            Order newOrder = await this.OrdersTable.Update(updatedOrder);
            return newOrder;
        }

        public async Task<IEnumerable<Order>> GetUserOrders(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            IEnumerable<Order> userOrders = await this.OrdersTable.GetUserOrders(userId);
            return userOrders;
        }
    }
}
