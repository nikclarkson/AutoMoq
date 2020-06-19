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

**Moq** allows us to `Verify` that a mocked method was called with particular values by using `It.Is<T>()`. In this test we want to make sure that the `AuditLogger` was called in both the Success and Failure cases. We would like to make our test a bit stronger than that and ensure that if the failure is due to a payment failure that the failure is reflected in the `OrderResponse` that returns from the `OrderController`. 
```csharp
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

    mockShippingService.Setup(shippingService => shippingService.Ship(It.IsAny<Order>()))
        .Returns(new ShippingResult
        {
            Success = isSuccessOrder
        });

    var order = new Order { PaymentMethod = "SuperCard" };
    ordersController.SubmitOrder(order);

    mockAuditLogger.Verify(al => al.LogOrder(
        It.IsAny<Order>(),
        It.Is<OrderResponse>(or => or.PaymentResult.Success == isSuccessOrder)));
}
```

The previous test is sufficient in its functionality, but it lacks clarity in the assertion if the test should ever fail. If the `mockAuditLogger.Verify` fails then we are presented with this error messge.
```csharp
Message: 
    Moq.MockException : 
    Expected invocation on the mock at least once, but was never performed: al => al.LogOrder(It.IsAny<Order>(), It.Is<OrderResponse>(or => or.PaymentResult.Success == False))
    
    Performed invocations:
    
       Mock<IAuditLogger:1> (al):
    
          IAuditLogger.LogOrder(Order, OrderResponse)
```
The test tells us what failed, but it doesn't tell us why. In other words we know what was expected, but we already knew this because we wrote the test. What a test written this way fails to tell us is what was the actual value of the `PaymentResult.Success` of the `OrderResponse` that was given to our `mockAuditLogger` as a parameter. 

We can use a **Moq Callback** to help us capture the actual value that is being passed between the mock objects. In this case we add a `.Callback()` to our `AuditLogger` mock setup.

```csharp
 mockAuditLogger.Setup(al => al.LogOrder(It.IsAny<Order>(), It.IsAny<OrderResponse>()))
                           .Callback<Order, OrderResponse>((o, or) => actualResult = or.PaymentResult.Success);
```
The `Callback()` gives you a chance to inspect the parameters being passed to the mocked method call. We are going to use that opporunity to capture the value we are interested in as the `actualResult` so that a failure message can include both *expected* and *actual* values.
```csharp
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

    mockShippingService.Setup(shippingService => shippingService.Ship(It.IsAny<Order>()))
        .Returns(new ShippingResult
        {
            Success = isSuccessOrder
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
```

Now the previous test provides us more visibility into what went wrong with our test case. The failure message makes our expected and actual values clear.
```csharp
Message: 
    Moq.MockException : Expected AuditLog with PaymentResult.Success == False but was True
    Expected invocation on the mock at least once, but was never performed: al => al.LogOrder(It.IsAny<Order>(), It.Is<OrderResponse>(or => or.PaymentResult.Success == False))
    
    Performed invocations:
    
       Mock<IAuditLogger:4> (al):
    
          IAuditLogger.LogOrder(Order, OrderResponse)
```

So far we have been making use of XUnit's `[InlineData]` attribute in building `[Theory]` unit test, but we can also get our test data sets from sources other that attributes. Using XUnit's `[MemberData]` attribute we can specify a data source. In this setup we will be providing a collection of arrays of objects.
```csharp
public static IEnumerable<object[]> Data =>
new List<object[]>
{
    new object[] { true },
    new object[] { false },
};
```

Now that we have our `Data` we can tell XUnit that we would like to use it with our `[Theory]`. Since we are using the same data as our previous `[Theory]`'s our test method will still required a single input parameter to pass the data in on each test iteration. 
```csharp
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

    mockShippingService.Setup(shippingService => shippingService.Ship(It.IsAny<Order>()))
        .Returns(new ShippingResult
        {
            Success = isSuccessOrder
        });

    var order = new Order();
    ordersController.SubmitOrder(order);

    mockAuditLogger.Verify(al => al.LogOrder(
        It.IsAny<Order>(),
        It.Is<OrderResponse>(or => or.PaymentResult.Success == isSuccessOrder)));
}


Often we want to know how our code will behave should a compenent that it depends on raises an exception. Moq gives us an easy way of setting up mocks such that when invoked they will throw a particular exception.
```csharp
[Fact]
public void Should_Verify_Shipping_Exception_Was_Thrown()
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

    mockShippingService.Setup(shippingService => shippingService.Ship(It.IsAny<Order>()))
        .Throws(It.IsAny<Exception>());

    var order = new Order { PaymentMethod = "SuperCard" };
    var response = ordersController.SubmitOrder(order);

    response.Success.Should().BeFalse();
}
```

The next example doesn't actually involve Moq, but it does deal with exceptions and how FluentAssertions can help us validate those exceptions. Here we have an actual `PaymentService` instance that we except to throw an exception when an invalid `PaymentMethod` is submitted. To accomplish this assertion we create an `Action` that when executed will invoke our test *Act*. This gives FluentAssertions the ability to execute the behavior in its own context so that it can catch any exceptions for matching against the specified assertion.
```csharp
[Fact]
public void Should_Throw_On_Invalid_Payment_Method_FluentValidations()
{
    var paymentService = new PaymentService();

    var order = new Order { PaymentMethod = string.Empty };

    Action pay = () => paymentService.Pay(order);
    pay.Should().Throw<Exception>();
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

