using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses.Payment.Events;
using _13_Wyd.RestApi.DB.Tables.Items;
using _13_Wyd.RestApi.DB.Tables.Payments;

namespace _13_Wyd.RestApi.Repositories
{
    public class PaymentsRepository : IPaymentsRepository
    {
        private readonly PaymentsQueueTS QueueTable;
        private readonly PaymentEventsTS PaymentsTable;
        private readonly ItemsTS ItemsTable;


        public PaymentsRepository(PaymentsQueueTS queueTable, PaymentEventsTS paymentsTable,
            ItemsTS itemsTable)
        {
            this.QueueTable = queueTable;
            this.PaymentsTable = paymentsTable;
            this.ItemsTable = itemsTable;
        }

        public async Task<PaymentEvent> AddNewPayment(PaymentEvent newPaymentEvent)
        {
            if (newPaymentEvent == null || !string.IsNullOrWhiteSpace(newPaymentEvent.PaymentId))
                return null;

            long invertedDateTimeTicks = DateTime.MaxValue.Ticks - newPaymentEvent.EventDateTime.Ticks;
            string newGuid = Guid.NewGuid().ToString();
            //dateTime.Ticks has a maximum of 19 digits
            string newPaymentID = $"PA-{invertedDateTimeTicks:D19}-{newGuid}";

            newPaymentEvent.PaymentId = newPaymentID;
            PaymentEvent createdPaymentEvent =
                await this.PaymentsTable.AddNewPaymentEvent(newPaymentEvent);

            if (createdPaymentEvent == null) return null; //some error occured

            Tuple<string, string> newQueuePayment = await this.QueueTable.EnqueueNewPayment(
                createdPaymentEvent.PaymentId, createdPaymentEvent.OrderId);

            if (newQueuePayment == null || !newQueuePayment.Item1.Equals(createdPaymentEvent.PaymentId)
                || !newQueuePayment.Item2.Equals(createdPaymentEvent.OrderId))
                return null;   //some error occured

            return createdPaymentEvent;
        }

        public async Task<PaymentEvent> DeletePayment(string paymentId, string orderId)
        {
            if (string.IsNullOrWhiteSpace(paymentId) || string.IsNullOrWhiteSpace(orderId))
                return null;

            PaymentEvent deletedPayment = await this.PaymentsTable.DeletePaymentEvent(
                            paymentId, orderId, itemId => this.ItemsTable.GetItem(itemId));

            Tuple<string, string> dequeuedPayment = await this.QueueTable.DequeuePayment(paymentId, orderId);

            return deletedPayment;
        }

        public async Task<IEnumerable<PaymentEvent>> GetOrderPayments(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return null;

            IEnumerable<PaymentEvent> orderPayments = await this.PaymentsTable.GetOrderPaymentEvents(
                orderId, itemId => this.ItemsTable.GetItem(itemId));

            return orderPayments;
        }

        public async Task<PaymentEvent> GetPayment(string paymentId, string orderId)
        {
            if (string.IsNullOrWhiteSpace(paymentId) || string.IsNullOrWhiteSpace(orderId))
                return null;

            PaymentEvent paymentEvent = await this.PaymentsTable.GetPaymentEvent(
                paymentId, orderId, itemId => this.ItemsTable.GetItem(itemId));

            return paymentEvent;
        }

        public async Task<IEnumerable<PaymentEvent>> GetQueuePayments()
        {
            IEnumerable<Tuple<string, string>> queueIDs = await this.QueueTable.GetQueue();

            if (queueIDs == null) return null;

            List<PaymentEvent> queue = new List<PaymentEvent>();

            foreach(var tuple in queueIDs)
            {
                string paymentId = tuple.Item1;
                string orderId = tuple.Item2;

                PaymentEvent payment = await this.GetPayment(paymentId, orderId);
                queue.Add(payment);
            }

            return queue;
        }

        public async Task<PaymentEvent> SetProcessedPayment(PaymentEvent processedPayment)
        {
            if (processedPayment == null || string.IsNullOrWhiteSpace(processedPayment.PaymentId)
                || string.IsNullOrWhiteSpace(processedPayment.OrderId) || !processedPayment.Processed)
                return null;

            PaymentEvent updatedPayment = await this.PaymentsTable.SetProcessedPayment(processedPayment);

            if (updatedPayment == null) return null;

            var tuple = await this.QueueTable.DequeuePayment(
                processedPayment.PaymentId, processedPayment.OrderId);

            if (tuple == null) return null;

            return updatedPayment;
        }
    }
}
