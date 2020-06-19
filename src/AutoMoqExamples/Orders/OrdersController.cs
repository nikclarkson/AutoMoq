using System;

namespace Orders
{
    public class OrdersController
    {
        private readonly IPaymentService _paymentService;
        private readonly IShippingService _shippingService;
        private readonly IAuditLogger _auditLogger;

        public OrdersController(IPaymentService paymentService, 
                                IShippingService shippingService, 
                                IAuditLogger auditLogger)
        {
            _paymentService = paymentService;
            _shippingService = shippingService;
            _auditLogger = auditLogger;
        }

        public OrderResponse SubmitOrder(Order order)
        {
            OrderResponse response;

            try
            {
                var paymentResult = _paymentService.Pay(order);
                ShippingResult shippingResult = null;

                if (paymentResult.Success)
                {
                    shippingResult = _shippingService.Ship(order);
                }

                response = new OrderResponse
                {
                    Success = paymentResult.Success && shippingResult.Success,
                    PaymentResult = paymentResult,
                    ShippingResult = shippingResult
                };

                _auditLogger.LogOrder(order, response);
            }
            catch (Exception)
            {
                response = new OrderResponse { Success = false };
            }

            return response;
        }
    }
}
