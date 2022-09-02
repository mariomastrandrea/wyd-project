using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace _13_Wyd.ModelClasses.Payment.Methods
{
    public class CreditOrDebitCard : IPaymentMethod
    {
        private readonly HttpClient CardPaymentApiClient;
        private PaymentType PaymentType;
        private string UserCardInfo;


        public CreditOrDebitCard(HttpClient cardPaymentApiClient, string userCardInfo)
        {
            this.CardPaymentApiClient = cardPaymentApiClient;
            this.PaymentType = PaymentType.CreditOrDebitCard;

            if (userCardInfo != null && userCardInfo != string.Empty)
                this.UserCardInfo = userCardInfo;
            else
                this.UserCardInfo = "(none)";
        }

        public async Task<bool> Pay(string userId, string orderId, string itemsInfo, decimal amount)
        {
            // 1) send confirmation code to user and await his confirmation:
            bool accepted = await this.SendConfirmationCodeToUser(userId, amount);

            if (!accepted) return false;

            // 2) process payment:

            PaymentInfo paymentInfo = new PaymentInfo(this.UserCardInfo, amount);

            try
            {
                var response = await this.CardPaymentApiClient.PostAsJsonAsync("api/card/pay", paymentInfo);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Status code: {response.StatusCode}\nReason: " +
                        $"{response.ReasonPhrase}\nHeaders: {response.Headers}\nContent: {response.Content}");
                    return false;
                }

                return true;    //payment ok
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return false;
            }
        }

        public PaymentType Type()
        {
            return this.PaymentType;
        }

        private Task<bool> SendConfirmationCodeToUser(string userId, decimal amount)
        {
            StringBuilder confirmationCodeMessage = new StringBuilder();

            string confirmationCode = this.GenerateNewCode();

            confirmationCodeMessage.Append($"A new Card payment is going to be charged by Wyd Platform.\n")
                   .Append($"Amount: {amount}\n")
                   .Append($"Confirmation code: {confirmationCode}");

            Console.WriteLine($"\n* User alert for payment confirmation *\n{confirmationCodeMessage}");

            // call to confirmation API
            bool accepted = true;
            // => User accepts with his code

            string acceptedInfo = accepted ? "accepted" : "didn't accept";
            Console.WriteLine($"\nUser {userId} {acceptedInfo} payment");

            return Task.FromResult(accepted);
        }

        private string GenerateNewCode()    //random code from:  000 000  to  999 999
        {
            int codeNumber = (int)(1000000 * new Random().NextDouble());

            return string.Format("{0:D6}", codeNumber);
        }
    }
}
