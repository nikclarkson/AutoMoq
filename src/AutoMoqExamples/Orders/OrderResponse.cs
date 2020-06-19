namespace Orders
{
    public class OrderResponse
    {
        public bool Success { get; set; }
        public PaymentResult PaymentResult { get; set; }
        public ShippingResult ShippingResult { get; set; }
    }
}
