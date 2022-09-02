using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses.Payment.Events;

namespace _13_Wyd.RestApi.Repositories
{
    public interface IPaymentsRepository
    {
        Task<PaymentEvent> AddNewPayment(PaymentEvent newPaymentEvent);
        Task<PaymentEvent> DeletePayment(string paymentId, string orderId);
        Task<PaymentEvent> GetPayment(string paymentId, string orderId);
        Task<IEnumerable<PaymentEvent>> GetQueuePayments();
        Task<IEnumerable<PaymentEvent>> GetOrderPayments(string orderId);
        Task<PaymentEvent> SetProcessedPayment(PaymentEvent processedPayment);
    }
}
