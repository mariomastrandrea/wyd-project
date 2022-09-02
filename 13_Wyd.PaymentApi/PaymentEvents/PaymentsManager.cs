using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.ModelClasses.DataStructures;
using _13_Wyd.ModelClasses.Payment.Events;
using _13_Wyd.ModelClasses.Payment.Methods;
using _13_Wyd.PaymentApi.PaymentEvents;
using _13_Wyd.PaymentApi.Services;

namespace _13_Wyd.PaymentApi.EventsPayment
{
    public class PaymentsManager
    {
        public static bool On = false;
        private static bool StoredQueue = false;
        private const int IntervalCheckInMilliSec = 10000;  // 10 seconds
        private readonly TimeSpan MaxRetryInterval = new TimeSpan(12, 0, 0); // 12 hours
        private readonly Dictionary<PaymentEvent, TimeSpan> FailedPayments;  // payments re-try timeSpans

        private readonly PaymentMethodFactory PaymentMethodFactory;
        private readonly IPriorityQueue<PaymentEvent> PaymentsQueue;   //* events queue *

        private readonly PaymentsDataService PaymentsDataService;


        public PaymentsManager(PaymentMethodFactory paymentMethodFactory,
            PaymentsDataService paymentsDataService)
        {
            this.FailedPayments = new Dictionary<PaymentEvent, TimeSpan>();
            this.PaymentMethodFactory = paymentMethodFactory;
            this.PaymentsQueue = new PaymentsQueueDecorator(new PriorityQueue<PaymentEvent>());
            this.PaymentsDataService = paymentsDataService;
        }

        public async Task<PaymentEvent> EnqueueNewPayment(PaymentEvent newPaymentEvent)
        {
            this.PaymentMethodFactory.RedirectAndSetPaymentMethodInfo(newPaymentEvent);

            PaymentEvent savedPaymentEvent = await this.PaymentsDataService.SaveNewPayment(newPaymentEvent);
            // now paymentEvent has ID set

            if (savedPaymentEvent == null) return null; //some error occured
            bool enqueued = this.PaymentsQueue.Enqueue(savedPaymentEvent);

            return enqueued ? savedPaymentEvent : null;
        }

        public async Task RunPayments() //this method starts the payment process (if it's off) and set On = true 
        {
            //first of all, retrieve stored payment event
            if (!StoredQueue)
            { 
                IEnumerable<PaymentEvent> storedPaymentEvents = await this.PaymentsDataService.GetStoredQueue();

                if (storedPaymentEvents == null)
                    throw new Exception("Error occured retrieving stored payment events");

                this.PaymentsQueue.EnqueueAllIfNotPresent(storedPaymentEvents);
                StoredQueue = true;
            }
            
            while(On)
            {
                await Task.Delay(IntervalCheckInMilliSec);
                PaymentEvent nextPayment;

                while(On && (nextPayment = this.PaymentsQueue.Dequeue()) != null)
                {
                    bool correctlyProcessed = await this.ProcessPayment(nextPayment);

                    if(!correctlyProcessed)
                    {
                        //schedule re-try
                        TimeSpan retryTimeSpan;

                        if (!this.FailedPayments.ContainsKey(nextPayment))
                        {
                            //re-try in a random amount of seconds, from 1 to 300 (5 minutes);
                            int retrySeconds = 1 + (int)(60 * 5 * new Random().NextDouble());
                            retryTimeSpan = new TimeSpan(0, 0, retrySeconds);
                        }
                        else
                        {
                            retryTimeSpan = this.FailedPayments[nextPayment];

                            if (!retryTimeSpan.Equals(this.MaxRetryInterval))
                            {
                                //exponential backoff
                                retryTimeSpan *= 2;

                                if (retryTimeSpan.CompareTo(this.MaxRetryInterval) > 0)
                                    retryTimeSpan = this.MaxRetryInterval;

                                this.FailedPayments.Remove(nextPayment);
                            }
                        }

                        this.FailedPayments.Add(nextPayment, retryTimeSpan);    //add new timeSpan

                        /*
                        DateTime oldDateTime = nextPayment.EventDateTime;
                        DateTime newDateTime = oldDateTime.Add(retryTimeSpan);
                        */

                        DateTime newDateTime = DateTime.Now.Add(retryTimeSpan);
                        nextPayment.EventDateTime = newDateTime;

                        this.PaymentsQueue.Enqueue(nextPayment);
                    }
                    else //payment correctly processed
                    {
                        if (this.FailedPayments.ContainsKey(nextPayment))
                            this.FailedPayments.Remove(nextPayment);
                    }
                }
            }
        }

        public async Task<bool> ProcessPayment(PaymentEvent paymentEvent)   
        {
            string orderId = paymentEvent.OrderId;
            string userId = paymentEvent.UserId;
            decimal totCharge = paymentEvent.TotalCharge;
            string itemsInfo = this.PrintPurchasedItemsInfo(paymentEvent.ItemsByQty);

            IPaymentMethod paymentMethod =
                this.PaymentMethodFactory.CreatePaymentMethod(paymentEvent.PaymentMethodInfo);  //factory

            bool paid = await paymentMethod.Pay(userId, orderId, itemsInfo, totCharge); 

            if (!paid)  return false;

            paymentEvent.Processed = true;
            await this.PaymentsDataService.SetProcessedPayment(paymentEvent);   //error?
            await this.EnqueueNextPaymentEvents(paymentEvent);              //error?

            return true;
        }

        private async Task EnqueueNextPaymentEvents(PaymentEvent paymentEvent)
        {
            string orderId = paymentEvent.OrderId;
            string userId = paymentEvent.UserId;
            PaymentMethodInfo paymentMethodInfo = paymentEvent.PaymentMethodInfo;
            DateTime paymentDateTime = paymentEvent.EventDateTime;

            Dictionary<TimeSpan, PaymentEvent> newPaymentEvents = new Dictionary<TimeSpan, PaymentEvent>();

            foreach (var pair in paymentEvent.ItemsByQty)
            {
                Item item = pair.Key;
                int qty = pair.Value;
                TimeSpan itemPaymentTimeSpan = item.PeriodTicks != null ?
                        TimeSpan.FromTicks((long)item.PeriodTicks) : TimeSpan.Zero;

                if (itemPaymentTimeSpan.CompareTo(TimeSpan.Zero) == 0)
                    continue;   // it's a one-shot item

                // it's a subscription item
                if (!newPaymentEvents.ContainsKey(itemPaymentTimeSpan))
                {
                    Dictionary<Item, int> itemsByQty = new Dictionary<Item, int>();
                    DateTime nextPaymentDateTime = paymentDateTime.Add(itemPaymentTimeSpan);

                    PaymentEvent newEvent = new PaymentEvent(null, orderId, nextPaymentDateTime, userId,
                                            itemsByQty, paymentMethodInfo, false);

                    newPaymentEvents.Add(itemPaymentTimeSpan, newEvent);
                }

                newPaymentEvents[itemPaymentTimeSpan].ItemsByQty.Add(item, qty);
            }

            if (newPaymentEvents.Any())
            {
                foreach (var pair in newPaymentEvents)
                {
                    TimeSpan timeSpan = pair.Key;
                    PaymentEvent paymEvent = pair.Value;

                    PaymentEvent savedPaymEvent =   //set new Payment ID
                        await this.PaymentsDataService.SaveNewPayment(paymEvent);

                    if (savedPaymEvent == null)
                        newPaymentEvents.Remove(timeSpan);
                    else
                        newPaymentEvents[timeSpan] = savedPaymEvent;    //update payment events with IDs set
                }
                
                this.PaymentsQueue.EnqueueAll(newPaymentEvents.Values);
            }
        }

        public string PrintPurchasedItemsInfo(Dictionary<Item, int> purchasedItems)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var pair in purchasedItems)
            {
                if (sb.Length > 0)
                    sb.Append('\n');

                Item item = pair.Key;
                int qty = pair.Value;

                sb.Append(item.ToString()).Append($" (x{qty})");
            }

            return sb.ToString();
        }
    }
}
