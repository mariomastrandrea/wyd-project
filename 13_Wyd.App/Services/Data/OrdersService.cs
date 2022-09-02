using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;

namespace _13_Wyd.App.Services.Data
{
    public class OrdersService
    {
        private readonly HttpClient DataClient;


        public OrdersService(HttpClient dataClient)
        {
            this.DataClient = dataClient;
        }

        public async Task<IEnumerable<Order>> GetOrders()
        {
            try
            {
                return await this.DataClient.GetFromJsonAsync<IEnumerable<Order>>($"api/orders");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<Order> GetOrder(string orderId)
        {
            try
            {
                return await this.DataClient.GetFromJsonAsync<Order>($"api/orders/{orderId}");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<Order> CreateOrder(Order newOrder)    //ok
        {
            try
            {
                var response = await this.DataClient.PostAsJsonAsync("api/orders", newOrder);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return null;
                }

                Order orderCreated = await response.Content.ReadFromJsonAsync<Order>();
                return orderCreated;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<Order> UpdateOrder(Order updatedOrder)    //check
        {
            try
            {
                var response = await this.DataClient.PutAsJsonAsync("api/orders", updatedOrder);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return null;
                }

                Order responseOrder = await response.Content.ReadFromJsonAsync<Order>();
                return responseOrder;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<Order> DeleteOrder(string orderId)    //ok
        { 
            try
            {
                var response = await this.DataClient.DeleteAsync($"api/orders/{orderId}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return null;
                }

                Order deletedOrder = await response.Content.ReadFromJsonAsync<Order>();
                return deletedOrder;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }
    }
}
