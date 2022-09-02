using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace _13_Wyd.ModelClasses.Payment.Methods
{
    public class Paypal : IPaymentMethod
    {
        private readonly decimal FixedBaseAmount = new decimal(0.10);   // 10 cents for each transaction
        private readonly HttpClient PaypalApiClient;
        private PaymentType PaymentType;
        private string PaypalAccountInfo;


        public Paypal(HttpClient paypalApiClient, string paypalAccountInfo)
        {
            this.PaypalApiClient = paypalApiClient;
            this.PaymentType = PaymentType.PayPal;

            if (string.IsNullOrWhiteSpace(paypalAccountInfo))
                this.PaypalAccountInfo = "(none)";
            else
                this.PaypalAccountInfo = paypalAccountInfo;
        }

        public async Task<bool> Pay(string userId, string orderId, string itemsInfo, decimal totPrice)
        {
            // 1) Notify user:
            this.NotifyUser(userId, orderId, itemsInfo, totPrice);

            // 2) process payments ($0,10 each):

            decimal paidAmount = decimal.Zero;
            int failuresCount = 0;
            bool paymentOk = true;

            while (paidAmount < totPrice)
            {
                if(failuresCount > 15)
                {
                    paymentOk = false;      //da migliorare
                    break;
                }

                PaymentInfo paymentInfo = new PaymentInfo(this.PaypalAccountInfo, FixedBaseAmount);
                
                try
                {
                    var response = await this.PaypalApiClient.PostAsJsonAsync("api/paypal/pay", paymentInfo);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                            $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");

                        failuresCount++;
                    }
                    else //payment ok
                        paidAmount += FixedBaseAmount;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.StackTrace);
                    failuresCount++;
                }
            }

            return paymentOk;
        }

        public PaymentType Type()
        {
            return this.PaymentType;
        }

        private void NotifyUser(string userId, string orderId, string itemsInfo, decimal totPrice)
        {
            StringBuilder message = new StringBuilder();

            message.Append($"A new paypal payment is going to be charged by Wyd Platform. Info:\n")
                   .Append($"User ID = {userId}\n")
                   .Append($"Order ID = {orderId}\n")
                   .Append($"Amount = {totPrice}\n")
                   .Append($"Items info = \n{itemsInfo}\n")
                   .Append($"Paypal account info = {this.PaypalAccountInfo}");

            Console.WriteLine($"\n* User alert *\n{message}");
        }
    }
}
