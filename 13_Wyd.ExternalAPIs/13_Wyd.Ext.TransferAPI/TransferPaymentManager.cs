using System;
using _13_Wyd.ModelClasses.Payment;

namespace _13_Wyd.Ext.TransferAPI
{
    public class TransferPaymentManager
    {
        public bool ProcessTransferPayment(PaymentInfo paymentInfo)
        {
            Console.WriteLine($"Transfer payment made correctly:\n{paymentInfo}");

            return true;
        }
    }
}