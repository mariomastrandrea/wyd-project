using System;
using System.Collections.Generic;
using _13_Wyd.ModelClasses.DataStructures;
using _13_Wyd.ModelClasses.Payment.Events;

namespace _13_Wyd.PaymentApi.PaymentEvents
{
    //Decorator Pattern
    public class PaymentsQueueDecorator : IPriorityQueue<PaymentEvent>
    {
        IPriorityQueue<PaymentEvent> PaymentsQueue;


        public PaymentsQueueDecorator(IPriorityQueue<PaymentEvent> paymentsQueue)
        {
            this.PaymentsQueue = paymentsQueue;
        }

        public bool Enqueue(PaymentEvent element)
        {
            return this.PaymentsQueue.Enqueue(element);
        }

        public void EnqueueAll(params PaymentEvent[] elements)
        {
            this.PaymentsQueue.EnqueueAll(elements);
        }

        public void EnqueueAll(IEnumerable<PaymentEvent> elements)
        {
            this.PaymentsQueue.EnqueueAll(elements);
        }

        public void EnqueueAllIfNotPresent(IEnumerable<PaymentEvent> elements)
        {
            this.PaymentsQueue.EnqueueAllIfNotPresent(elements);
        }

        public PaymentEvent Peek()
        {
            PaymentEvent nextPayment = this.PaymentsQueue.Peek();

            if(nextPayment == null) //no more payments in queue
                return null;

            DateTime dateTimeNextPayment = nextPayment.EventDateTime;

            if (dateTimeNextPayment.IsInTheFuture())
                return null;    //there are only future payments

            //payment has to be processed
            return nextPayment;
        }

        public PaymentEvent Dequeue()
        {
            PaymentEvent nextPayment = this.PaymentsQueue.Peek();

            if (nextPayment == null) //no more payments in queue
                return null;

            DateTime dateTimeNextPayment = nextPayment.EventDateTime;

            if (dateTimeNextPayment.IsInTheFuture())
                return null;    //there are only future payments

            //payment has to be processed
            return this.PaymentsQueue.Dequeue();
        }
    }

    internal static class DateTimeExtensions
    {
        internal static bool IsInTheFuture(this DateTime thisDateTime)
        {
            return thisDateTime.CompareTo(DateTime.Now) > 0;
        }
    }
}
