using Moq;
using Orders;
using Xunit;

namespace Examples.Moq
{
    public class MoqBasedTests
    {
        [Fact]
        public void Should_Ship_Order_When_Payment_Successful()
        {
            var mockShippingService = new Mock<IShippingService>();
            var mockAuditLogger = new Mock<IAuditLogger>();
            var mockPaymentService = new Mock<IPaymentService>();

            var ordersController = new OrdersController(
                mockPaymentService.Object,
                mockShippingService.Object,
                mockAuditLogger.Object);

            mockPaymentService.Setup(paymentService => paymentService.Pay(It.IsAny<Order>()))
                .Returns(new PaymentResult 
                {
                    Success = true 
                });

            
            var order = new Order();
            ordersController.SubmitOrder(order);

            mockShippingService.Verify(shippingService => shippingService.Ship(It.IsAny<Order>()), Times.Once);
        }
    }
}
