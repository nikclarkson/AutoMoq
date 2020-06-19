namespace Orders
{
    public interface IShippingService
    {
        ShippingResult Ship(Order order);
    }
}
