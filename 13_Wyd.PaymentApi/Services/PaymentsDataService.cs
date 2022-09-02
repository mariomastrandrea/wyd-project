using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using _13_Wyd.ModelClasses.Payment.Events;
using Newtonsoft.Json;

namespace _13_Wyd.PaymentApi.Services
{
    public class PaymentsDataService
    {
        private readonly HttpClient DataClient;


        public PaymentsDataService(HttpClient dataClient)
        {
            this.DataClient = dataClient;
        }

        public async Task<PaymentEvent> SaveNewPayment(PaymentEvent newPaymentEvent)
        { 
            try
            {
                string serializedNewPaymentEvent = JsonConvert.SerializeObject(newPaymentEvent);

                var response = await this.DataClient.PostAsJsonAsync("api/payments", serializedNewPaymentEvent);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return null;
                }

                string serializedPaymentCreated = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(serializedPaymentCreated)) return null;

                PaymentEvent paymentCreated = JsonConvert.DeserializeObject<PaymentEvent>(serializedPaymentCreated);

                return paymentCreated;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<bool> SetProcessedPayment(PaymentEvent paymentEvent)
        {
            try
            {
                string serializedPaymentEvent = JsonConvert.SerializeObject(paymentEvent);

                var response = await this.DataClient.PutAsJsonAsync("api/payments", serializedPaymentEvent);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return false;
                }

                string serializedResponsePayment = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(serializedResponsePayment)) return false;

                PaymentEvent responsePayment =
                    JsonConvert.DeserializeObject<PaymentEvent>(serializedResponsePayment);

                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<IEnumerable<PaymentEvent>> GetStoredQueue()
        {
            try
            {
                string serializedStoredQueue = await this.DataClient.GetStringAsync($"api/payments/queue");

                if (string.IsNullOrWhiteSpace(serializedStoredQueue))
                    return null;

                IEnumerable<PaymentEvent> storedQueue =
                    JsonConvert.DeserializeObject<IEnumerable<PaymentEvent>>(serializedStoredQueue);

                return storedQueue;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return null;
            }
        }
    }
}
