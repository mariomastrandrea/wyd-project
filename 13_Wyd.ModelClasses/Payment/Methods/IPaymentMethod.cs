using System;
using System.Threading.Tasks;

namespace _13_Wyd.ModelClasses.Payment.Methods
{
    public interface IPaymentMethod
    {
        PaymentType Type();
        Task<bool> Pay(string userId, string orderId, string itemsInfo, decimal totPrice);
    }
}
