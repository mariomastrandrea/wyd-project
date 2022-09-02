using System;
namespace _13_Wyd.ModelClasses.Payment
{
    public class PaymentInfo
    {
        public string PaymentMethodInfo { get; set; }
        public decimal TotalCharge { get; set; }


        public PaymentInfo(string paymentMethodInfo, decimal totalCharge)
        {
            this.PaymentMethodInfo = paymentMethodInfo;
            this.TotalCharge = totalCharge;
        }

        public override string ToString()
        {
            return $"PaymentMethodInfo={PaymentMethodInfo}; TotalCharge=${TotalCharge}";
        }
    }
}
