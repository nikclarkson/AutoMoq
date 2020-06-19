namespace Orders
{
    public interface IPaymentService
    {
        PaymentResult Pay(Order order);
    }
}
