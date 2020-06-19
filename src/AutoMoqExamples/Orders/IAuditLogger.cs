namespace Orders
{
    public interface IAuditLogger
    {
        void LogOrder(Order order, OrderResponse response);
    }
}
