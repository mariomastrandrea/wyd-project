using System;
using _13_Wyd.ModelClasses.Payment;

namespace _13_Wyd.Ext.CreditOrDebitCardAPI
{
    public class CardPaymentManager
    {
        public bool ProcessCardPayment(PaymentInfo paymentInfo)
        {
            Console.WriteLine($"Card payment made correctly:\n{paymentInfo}");
            return true;
        }
    }
}
