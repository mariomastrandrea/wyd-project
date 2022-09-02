using System;

namespace _13_Wyd.ModelClasses.Payment.Methods
{
    public class PaymentMethodInfo
    {
        public PaymentType PaymentType { get; set; }
        public string PaymentMethodUserInfo { get; set; }


        public PaymentMethodInfo(PaymentType paymentType, string paymentMethodUserInfo)
        {
            this.PaymentType = paymentType;
            this.PaymentMethodUserInfo = paymentMethodUserInfo;
        }
    }
}
