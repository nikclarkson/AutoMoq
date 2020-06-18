using System;

namespace Orders
{
    public class OrdersController
    {
        private readonly IPaymentService _paymentService;
        private readonly IShippingService _shippingService;
        private readonly IAuditLogger _auditLogger;

        public OrdersController(IPaymentService paymentService, IShippingService shippingService, IAuditLogger auditLogger)
        {
            _paymentService = paymentService;
            _shippingService = shippingService;
            _auditLogger = auditLogger;
        }

        public OrderResponse SubmitOrder(Order order)
        {
            var paymentResult = _paymentService.Pay(order);
            ShippingResult shippingResult = null;

            if (paymentResult.Success)
            {
                shippingResult = _shippingService.Ship(order);
            }

            var response = new OrderResponse { PaymentResult = paymentResult, ShippingResult = shippingResult };

            _auditLogger.LogOrder(order, response);

            return response;
        }
    }

    public interface IPaymentService
    {
        PaymentResult Pay(Order order);
    }
    public interface IShippingService
    {
        ShippingResult Ship(Order order);
    }
    public interface IAuditLogger
    {
        void LogOrder(Order order, OrderResponse response);
    }

    public class OrderResponse
    {
        public PaymentResult PaymentResult { get; set; }
        public ShippingResult ShippingResult { get; set; }
    }
    public class Order { }

    public class PaymentResult
    {
        public bool Success { get; set; }
    }
    public class ShippingResult { }
}
