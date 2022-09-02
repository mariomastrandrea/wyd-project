using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;

namespace _13_Wyd.RestApi.Repositories
{
    public interface IOrdersRepository
    {
        Task<IEnumerable<Order>> GetOrders();
        Task<Order> GetOrder(string id);
        Task<Order> CreateOrder(Order newOrder);
        Task<Order> UpdateOrder(Order updatedOrder);
        Task<Order> DeleteOrder(string id);
        Task<IEnumerable<Order>> GetUserOrders(string userId);
    }
}
