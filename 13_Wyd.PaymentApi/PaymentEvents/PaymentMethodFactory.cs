using System;
using System.Net.Http;
using _13_Wyd.ModelClasses.Payment.Events;
using _13_Wyd.ModelClasses.Payment.Methods;

namespace _13_Wyd.PaymentApi.PaymentEvents
{
    public class PaymentMethodFactory
    {
        private readonly HttpClient CardApiClient;
        private readonly HttpClient PaypalApiClient;
        private readonly HttpClient TransferApiClient;


        public PaymentMethodFactory(IHttpClientFactory clientFactory)
        {
            this.CardApiClient = clientFactory.CreateClient("CardClient");
            this.PaypalApiClient = clientFactory.CreateClient("PaypalClient");
            this.TransferApiClient = clientFactory.CreateClient("TransferClient");
        }

        public IPaymentMethod CreatePaymentMethod(PaymentMethodInfo paymentMethodInfo)
        {
            IPaymentMethod paymentMethod;

            switch (paymentMethodInfo.PaymentType)
            {
                case PaymentType.CreditOrDebitCard:

                    string cardInfo = paymentMethodInfo.PaymentMethodUserInfo;
                    paymentMethod = new CreditOrDebitCard(this.CardApiClient, cardInfo);
                    break;

                case PaymentType.PayPal:

                    string paypalAccountInfo = paymentMethodInfo.PaymentMethodUserInfo;
                    paymentMethod = new Paypal(this.PaypalApiClient, paypalAccountInfo);
                    break;

                case PaymentType.Transfer:

                    string transferUserInfo = paymentMethodInfo.PaymentMethodUserInfo;
                    paymentMethod = new Transfer(this.TransferApiClient, transferUserInfo);
                    break;

                default:
                    throw new ArgumentNullException();
            }

            return paymentMethod;
        }

        public void RedirectAndSetPaymentMethodInfo(PaymentEvent newPaymentEvent)
        {
            PaymentType paymentType = newPaymentEvent.PaymentMethodInfo.PaymentType;

            switch (paymentType)
            {
                case PaymentType.CreditOrDebitCard:

                    string cardInfo = this.RedirectToCardServiceView();
                    newPaymentEvent.PaymentMethodInfo.PaymentMethodUserInfo = cardInfo;
                    break;

                case PaymentType.PayPal:

                    string paypalAccountInfo = this.RedirectToPaypalView();
                    newPaymentEvent.PaymentMethodInfo.PaymentMethodUserInfo = paypalAccountInfo;
                    break;

                case PaymentType.Transfer:

                    string transferUserInfo = this.RedirectToTransferView();
                    newPaymentEvent.PaymentMethodInfo.PaymentMethodUserInfo = transferUserInfo;
                    break;
            }
        }

        public string RedirectToTransferView()
        {
            //collect user's transfer informations
            string transferInfo = string.Empty;

            return transferInfo;
        }

        public string RedirectToPaypalView()
        {
            //collect user's paypal account informations
            string paypalAccountInfo = string.Empty;

            return paypalAccountInfo;
        }

        public string RedirectToCardServiceView()
        {
            //collect user's card informations
            string cardInfo = string.Empty;

            return cardInfo;
        }
    } 
}
