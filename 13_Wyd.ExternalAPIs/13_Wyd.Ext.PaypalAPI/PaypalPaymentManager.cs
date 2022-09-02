using System;
using _13_Wyd.ModelClasses.Payment;

namespace _13_Wyd.Ext.PaypalAPI
{
    public class PaypalPaymentManager
    {
        public bool ProcessPaypalPayment(PaymentInfo paymentInfo)
        {
            Console.WriteLine($"Paypal payment made correctly:\n{paymentInfo}");

            return true;
        }
    }
}
