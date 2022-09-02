using System;
using System.Collections.Generic;
using _13_Wyd.ModelClasses;
using _13_Wyd.ModelClasses.Payment.Methods;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace _13_Wyd.RestApi.DB.Entities.Orders
{
    public class OrderEntity : TableEntity
    {
        public string PurchasedItemsQtyJson { get; set; } //converted Dictionary<string,int> <--> string(JSON)
        public double TotalCost { get; set; }       //converted decimal <--> double
        public string ShippingAddress { get; set; }
        public int PaymentTypeInt { get; set; }    //converted PaymentType <--> int
        public bool? Gift { get; set; }
        public DateTime OrderTimeStamp { get; set; }
        public bool? Delivered { get; set; }
        public bool? Canceled { get; set; }

        public OrderEntity() { }

        public OrderEntity(Order order) 
        {
            this.PartitionKey = order.BuyerId;
            this.RowKey = order.Id;

            this.PurchasedItemsQtyJson = JsonConvert.SerializeObject(order.PurchasedItemsQty);
            this.TotalCost = (double)order.TotalCost;
            this.ShippingAddress = order.ShippingAddress;
            this.PaymentTypeInt = (int)order.PaymentType;
            this.Gift = order.Gift;
            this.OrderTimeStamp = (DateTime)order.TimeStamp;
            this.Delivered = order.Delivered;
            this.Canceled = order.Canceled;
        }

        public Order ToOrder()
        {
            string orderId = this.RowKey;
            string userId = this.PartitionKey;

            Dictionary<string, int> purchasedItemsQty =
                JsonConvert.DeserializeObject<Dictionary<string, int>>(this.PurchasedItemsQtyJson);

            decimal totalCost = (decimal)this.TotalCost;
            PaymentType paymentType = Enum.Parse<PaymentType>(this.PaymentTypeInt.ToString());

            Order order = new Order(orderId, userId, purchasedItemsQty, totalCost,
                this.ShippingAddress, paymentType, this.Gift, this.OrderTimeStamp,
                this.Delivered, this.Canceled);

            return order;
        }
    }
}
