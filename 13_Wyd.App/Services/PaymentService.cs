using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses;
using _13_Wyd.ModelClasses.Payment.Events;
using _13_Wyd.ModelClasses.Payment.Methods;
using Newtonsoft.Json;

namespace _13_Wyd.App.Services
{
    public class PaymentService
    {
        private readonly HttpClient PaymentApiClient;

        public PaymentService(HttpClient paymentApiClient)
        {
            this.PaymentApiClient = paymentApiClient;
        }

        public async Task<PaymentEvent> ProcessPayment(PaymentType? paymentType, string orderId,  //ok
                                    string userId, Dictionary<Item,int> itemsByQty)
        {
            if (paymentType == null || orderId == null || userId == null || itemsByQty == null
                || !itemsByQty.Any())
                return null;

            DateTime paymentDateTime = DateTime.Now;
            PaymentMethodInfo paymentMethodInfo = new PaymentMethodInfo((PaymentType)paymentType, null);

            PaymentEvent paymentEvent = new PaymentEvent(null, orderId, paymentDateTime, userId,
                                        itemsByQty, paymentMethodInfo, false);

            string serializedPaymentEvent = JsonConvert.SerializeObject(paymentEvent);

            try
            {
                var response = await this.PaymentApiClient.PostAsJsonAsync("api/payments", serializedPaymentEvent);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return null;
                }

                string serializedEnqueuedPayment = await response.Content.ReadAsStringAsync();
                PaymentEvent enqueuedPayment = JsonConvert.DeserializeObject<PaymentEvent>(serializedEnqueuedPayment);

                return enqueuedPayment;
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<string> StartProcessingPayments()
        {
            try
            {
                string response = await this.PaymentApiClient.GetStringAsync("api/payments/run");

                if (response == null) return null;

                return response;
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<string> StopProcessingPayments()
        {
            try
            {
                string response = await this.PaymentApiClient.GetStringAsync("api/payments/stop");

                if (response == null) return null;

                return response;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }
    }
}
