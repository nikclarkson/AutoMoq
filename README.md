# Writing tests with Xunit, Moq, AutoFixture & FluentValidations
Unit Testing examples leveraging Xunit, Moq, AutoFixture & FluentValidations packages.

**XUnit** is a unit testing framework.

**Moq** is a mocking library for .Net. Moq allows us to quickly and easily setup mocks and make verifications against those instances in our unit tests. 

**AutoFixture** makes object creation and data generation a breeze. Allows for customizations and integrations with other testing tools like Moq.

**FluentValidations** provides a fluent API for assertions in  your unit tests.

## Moq Examples

Imagine for a moment that we are about to test the world's greatest ordering system...
```csharp
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
```

We would like to test the behavior that the order is sent over to the shipping service once the customer has successfully paid for the order. In order to accomplish this we will want an actual concrete instance of the OrdersController, but everything it depends on can be mocks.

Creating Mocks with Moq is really simple.
```csharp
var mock = new Mock<string>();
```
That's it! Now we have a mock in hand, but what does that give us?
```csharp
mock.Object; // this provides us the instance of the mocked type to pass around and interact with
mock.Setup(...); // Setup allows us to implement behaviors on methods and properties of the mock
mock.Verify(...); // Verify allows us to assert that certain interactions with the mock instance occurred. 
```

Now that we know the basic hooks that Moq gives us we can test our award winning OrdersController.  In order to test the `OrdersController` behavior we will want to `Setup()` our `PaymentService` mock to return a success. Then to assert the controller works as advertised we can `Verify()` that the `ShippingService` mock was called appropriately.
```csharp 
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
```
Now that we have tested that the `OrdersController` properly calls the `ShippingService` when the `PaymentService` call is succesful we should test that the opposite is enforced. The failure scenario is effectively the same test but with different **Expected** and **Actual** values.  The previous *XUnit* `[Fact]` tests a single specific scenario, but by attributing our test method as a `[Theory]` instead gives us the chance to provide data to the test that we can use to modify the expectation.
```csharp
[Theory]
[InlineData(true)]
[InlineData(false)]
public void Should_Only_Call_Ship_Order_On_Successful_Payment(bool isSuccessOrder)
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
```


## AutoFixture

### Data Generation

### Dependency Injection 

### Working with Moq

## AutoFixture Customizations

### AutoMoqCustomization

### ICustomization(s)

### ISpecimenBuilder Customizations

