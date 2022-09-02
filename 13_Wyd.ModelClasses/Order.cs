using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using _13_Wyd.ModelClasses.Payment.Methods;

namespace _13_Wyd.ModelClasses
{
    public class Order
    {
        public string Id { get; set; }
        public string BuyerId { get; set; }
        public Dictionary<string, int> PurchasedItemsQty { get; set; }
        public decimal? TotalCost { get; set; }
        public string ShippingAddress { get; set; }
        public PaymentType? PaymentType { get; set; }
        public bool? Gift { get; set; }
        public DateTime? TimeStamp { get; set; }
        public bool? Delivered { get; set; }
        public bool? Canceled { get; set; }


        [JsonConstructor]
        public Order(string id, string buyerId, Dictionary<string, int> purchasedItemsQty,
            decimal? totalCost, string shippingAddress, PaymentType? paymentType,
            bool? gift, DateTime? timeStamp, bool? delivered, bool? canceled)
        {
            this.Id = id;
            this.BuyerId = buyerId;
            this.PurchasedItemsQty = purchasedItemsQty;
            this.TotalCost = totalCost;
            this.ShippingAddress = shippingAddress;
            this.PaymentType = paymentType;
            this.Gift = gift;
            this.TimeStamp = timeStamp;
            this.Delivered = delivered;
            this.Canceled = canceled;
        }

        public void Deliver()
        {
            this.Delivered = true;
        }

        public void Cancel()
        {
            this.Canceled = true;
        }

        public override bool Equals(object obj)
        {
            return obj is Order order &&
                   Id == order.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public void Update(Order newOrder)
        {
            string newBuyerId = newOrder.BuyerId;
            Dictionary<string, int> newPurchasedItemsQty = newOrder.PurchasedItemsQty;
            decimal? newTotalCost = newOrder.TotalCost;
            string newShippingAddress = newOrder.ShippingAddress;
            PaymentType? newPaymentType = newOrder.PaymentType;
            bool? newGift = newOrder.Gift;
            DateTime? newTimeStamp = newOrder.TimeStamp;
            bool? newDelivered = newOrder.Delivered;
            bool? newCanceled = newOrder.Canceled;

            if (newBuyerId != null)
                this.BuyerId = newBuyerId;

            if (newPurchasedItemsQty != null)
                this.PurchasedItemsQty = newPurchasedItemsQty;

            if (newTotalCost != null)
                this.TotalCost = newTotalCost;

            if (newShippingAddress != null)
                this.ShippingAddress = newShippingAddress;

            if (newPaymentType != null)
                this.PaymentType = newPaymentType;

            if (newGift != null)
                this.Gift = newGift;

            if (newTimeStamp != null)
                this.TimeStamp = newTimeStamp;

            if (newDelivered != null)
                this.Delivered = newDelivered;

            if (newCanceled != null)
                this.Canceled = newCanceled;
        }

        public override string ToString()
        {
            return $"Id = {this.Id}, Buyer = {this.BuyerId}\n" +
                $"Purchased items = {string.Join(", ", this.PurchasedItemsQty.Select(pair => $"{pair.Key}: {pair.Value}"))}\n" +
                $"Total cost = {this.TotalCost}\nShipping address = {this.ShippingAddress}\nPayment type = {this.PaymentType?.ToString()}\n" +
                $"Timestamp = {this.TimeStamp}\nGift = {this.Gift}, Delivered = {this.Delivered}, Canceled = {this.Canceled}";
        }
    }
}
