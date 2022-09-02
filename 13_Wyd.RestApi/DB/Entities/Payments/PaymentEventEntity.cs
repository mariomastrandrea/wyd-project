using System;
using System.Collections.Generic;
using System.Linq;
using _13_Wyd.ModelClasses;
using _13_Wyd.ModelClasses.Payment.Events;
using _13_Wyd.ModelClasses.Payment.Methods;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace _13_Wyd.RestApi.DB.Entities.Payments
{
    public class PaymentEventEntity : TableEntity
    {
        public DateTime? EventDateTime { get; set; }
        public string UserId { get; set; }
        public string ItemsIDsByQtyJson { get; set; } //conversion Dictionary<string,int> <--> string(JSON) Dictionary<Item,int>
        public double? TotalCharge { get; set; }      //conversion decimal <--> double
        public int? PaymentTypeInt { get; set; }  //conversion PaymentType <--> int
        public string PaymentMethodUserInfo { get; set; }
        public bool? Processed { get; set; }


        public PaymentEventEntity() { }

        public PaymentEventEntity(PaymentEvent newEvent)
        {
            this.PartitionKey = newEvent.PaymentId;
            this.RowKey = newEvent.OrderId;

            this.EventDateTime = newEvent.EventDateTime;
            this.UserId = newEvent.UserId;

            Dictionary<string, int> itemsIDsByQty =
                newEvent.ItemsByQty.ToDictionary(pair => pair.Key.Id, pair => pair.Value);

            this.ItemsIDsByQtyJson = JsonConvert.SerializeObject(itemsIDsByQty);
            this.TotalCharge = (double)newEvent.TotalCharge;
            this.PaymentTypeInt = (int)newEvent.PaymentMethodInfo.PaymentType;
            this.PaymentMethodUserInfo = newEvent.PaymentMethodInfo.PaymentMethodUserInfo;
            this.Processed = newEvent.Processed;
        }

        public Dictionary<string, int> GetItemsIDsByQty()
        {
            return JsonConvert.DeserializeObject<Dictionary<string, int>>(this.ItemsIDsByQtyJson);
        }

        public PaymentEvent ToPaymentEvent(Dictionary<Item,int> itemsByQty)
        {
            string paymentId = this.PartitionKey;
            string orderId = this.RowKey;
            PaymentType paymentType = Enum.Parse<PaymentType>(this.PaymentTypeInt.ToString());
            PaymentMethodInfo paymentMethodInfo = new PaymentMethodInfo(paymentType, this.PaymentMethodUserInfo);

            PaymentEvent paymentEvent = new PaymentEvent(paymentId, orderId, (DateTime)this.EventDateTime,
                this.UserId, itemsByQty, paymentMethodInfo, (bool)this.Processed);

            return paymentEvent;
        }
    }
}
