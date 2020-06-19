using Moq;
using Orders;
using System.Collections.Generic;
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_Call_Ship_Order_On_Successful_Payment(bool isSuccessOrder)
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
                    Success = isSuccessOrder
                });

            var order = new Order();
            ordersController.SubmitOrder(order);

            mockShippingService.Verify(
                shippingService => shippingService.Ship(It.IsAny<Order>()), 
                isSuccessOrder ? Times.Once() : Times.Never());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_Call_Audit_Logger_When_Order_Attempted(bool isSuccessOrder)
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
                    Success = isSuccessOrder
                });

            var order = new Order();
            ordersController.SubmitOrder(order);

            mockAuditLogger.Verify(al => al.LogOrder(
                It.IsAny<Order>(),
                It.Is<OrderResponse>(or => or.PaymentResult.Success == isSuccessOrder)));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_Call_Audit_Logger_When_Order_Attempted_WithFailMessage(bool isSuccessOrder)
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
                    Success = !isSuccessOrder // intentionally incorrect to demonstration failure message
                });

            var actualResult = false;
            mockAuditLogger.Setup(al => al.LogOrder(It.IsAny<Order>(), It.IsAny<OrderResponse>()))
                           .Callback<Order, OrderResponse>((o, or) => actualResult = or.PaymentResult.Success);

            var order = new Order();
            ordersController.SubmitOrder(order);

            mockAuditLogger.Verify(al => al.LogOrder(
                It.IsAny<Order>(),
                It.Is<OrderResponse>(or => or.PaymentResult.Success == isSuccessOrder)), 
                $"Expected AuditLog with PaymentResult.Success == {isSuccessOrder} but was {actualResult}");
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void Should_Call_Audit_Logger_When_Order_Attempted_MemberData(bool isSuccessOrder)
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
                    Success = isSuccessOrder
                });

            var order = new Order();
            ordersController.SubmitOrder(order);

            mockAuditLogger.Verify(al => al.LogOrder(
                It.IsAny<Order>(),
                It.Is<OrderResponse>(or => or.PaymentResult.Success == isSuccessOrder)));
        }

        public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            new object[] { true },
            new object[] { false },
        };
    }
}
