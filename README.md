# Writing tests with Moq & AutoFixture
Testing example leveraging Moq and AutoFixture

## Moq
Moq is a mocking library for .Net. Moq allows us to quickly and easily setup mocks and make verifications against those instances in our unit tests. 

Imagine for a moment that we are about to test the world's greatest ordering system...
```csharp
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
```

We would like to test the behavior that the order is sent over to the shipping department once the customer has successfully paid for the order. We will want an actual concrete instance of the OrdersController, but everything it depends on can be mocks at this point.
```csharp 
[Fact]
public void Should_Ship_Order_When_Payment_Successful()
{
    var mockOrder = new Mock<Order>();
    var mockShippingService = new Mock<IShippingService>();
    var mockAuditLogger = new Mock<IAuditLogger>();

    var mockPaymentService = new Mock<IPaymentService>();
    
    mockPaymentService.Setup(paymentService => paymentService.Pay(It.IsAny<Order>()))
        .Returns(new PaymentResult 
        {
            Success = true 
        });

    var ordersController = new OrdersController(
        mockPaymentService.Object,
        mockShippingService.Object,
        mockAuditLogger.Object);

    ordersController.SubmitOrder(mockOrder.Object);

    mockShippingService.Verify(shippingService => shippingService.Ship(It.IsAny<Order>()), Times.Once);
}
```


## AutoFixture

### Data Generation

### Dependency Injection 

### Working with Moq

## AutoFixture Customizations

### AutoMoqCustomization

### ICustomization(s)

### ISpecimenBuilder Customizations

