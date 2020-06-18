# Writing tests with Moq & AutoFixture
Testing example leveraging Moq and AutoFixture

## Moq
Moq is a mocking library for .Net

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

