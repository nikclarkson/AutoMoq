using System;

namespace Orders
{
    public interface IPaymentService
    {
        PaymentResult Pay(Order order);
    }

    public class PaymentService : IPaymentService
    {
        public PaymentResult Pay(Order order)
        {
            if (string.IsNullOrEmpty(order.PaymentMethod))
            {
                throw new Exception("Must provide valid payment method.");
            }

            var result = new PaymentResult();
            result.Success = CallPaymentVendor(order.PaymentMethod);

            return new PaymentResult();
        }

        private bool CallPaymentVendor(string paymentMethod)
        {
            return true;
        }
    }
}
