using System;
using System.Collections.Generic;
using System.Linq;
using _13_Wyd.ModelClasses.Payment.Methods;

namespace _13_Wyd.ModelClasses.Payment.Events
{
    public class PaymentEvent : IComparable<PaymentEvent>
    {
        public string PaymentId { get; set; }
        public string OrderId { get; set;  }
        public DateTime EventDateTime { get; set; }
        public string UserId { get; set;  }
        public Dictionary<Item, int> ItemsByQty { get; set; }
        public decimal TotalCharge { get; set; }
        public PaymentMethodInfo PaymentMethodInfo { get; set; }
        public bool Processed { get; set; }


        public PaymentEvent() { }

        public PaymentEvent(string paymentId, string orderId, DateTime dateTime, string userId,
                    Dictionary<Item, int> itemsByQty, PaymentMethodInfo paymentMethodInfo, bool processed)
        {
            this.PaymentId = paymentId;
            this.OrderId = orderId;
            this.EventDateTime = dateTime;
            this.UserId = userId;
            this.ItemsByQty = itemsByQty;
            this.PaymentMethodInfo = paymentMethodInfo;

            this.TotalCharge = this.ItemsByQty.Sum(pair =>
            {
                Item i = pair.Key;
                decimal price = (decimal)i.Price;
                int qty = pair.Value;

                return price * new decimal(qty);
            });

            this.Processed = processed;
        }

        private string Print(Dictionary<Item, int> itemList)
        {
            return string.Join(", ", itemList.Select(pair => $"{pair.Key.Id}: {pair.Value}"));
        }

        public int CompareTo(PaymentEvent other)
        {
            return this.EventDateTime.CompareTo(other.EventDateTime);
        }

        public override bool Equals(object obj)
        {
            return obj is PaymentEvent @event &&
                   PaymentId == @event.PaymentId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PaymentId);
        }

        public override string ToString()
        {
            return $"Payment Id = {this.PaymentId}, OrderId = {this.OrderId}, DateTime = {this.EventDateTime}, " +
                $"User Id = {this.UserId}\nItem quantities = {this.Print(this.ItemsByQty)}\nTotal charge = {this.TotalCharge}\n " +
                $"Payment type = {this.PaymentMethodInfo.PaymentType}, Payment method user info: " +
                $"{this.PaymentMethodInfo.PaymentMethodUserInfo}\n Processed = {this.Processed}";
        }
    }
}
