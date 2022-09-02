using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace _13_Wyd.ModelClasses.Payment.Methods
{
    public class Transfer : IPaymentMethod
    {
        private readonly HttpClient TransferApiClient;
        private PaymentType PaymentType;
        private string TransferUserInfo;


        public Transfer(HttpClient transferApiClient, string transferUserInfo)
        {
            this.TransferApiClient = transferApiClient;
            this.PaymentType = PaymentType.Transfer;
            this.TransferUserInfo = transferUserInfo;

            if (transferUserInfo != null && transferUserInfo != string.Empty)
                this.TransferUserInfo = transferUserInfo;
            else
                this.TransferUserInfo = "(none)";
        }

        public async Task<bool> Pay(string userId, string orderId, string itemsInfo, decimal totPrice)
        {
            // 1) Notify user:
            this.NotifyUser(userId, orderId, itemsInfo, totPrice);

            // 2) process payment:

            PaymentInfo paymentInfo = new PaymentInfo(this.TransferUserInfo, totPrice);

            try
            {
                var response = await this.TransferApiClient.PostAsJsonAsync("api/transfer/pay", paymentInfo);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return false;
                }

                return true;    //payment ok
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public PaymentType Type()
        {
            return this.PaymentType;
        }

        private void NotifyUser(string userId, string orderId, string itemsInfo, decimal totPrice)
        {
            StringBuilder message = new StringBuilder();

            message.Append($"A new Transfer payment is going to be charged by Wyd Platform. Info:\n")
                   .Append($"User ID = {userId}\n")
                   .Append($"Order ID = {orderId}\n")
                   .Append($"Amount = {totPrice}\n")
                   .Append($"Items info = \n{itemsInfo}\n")
                   .Append($"Transfer info = \n{this.TransferUserInfo}");

            Console.WriteLine($"\n* User alert *\n{message}");
        }
    }
}
