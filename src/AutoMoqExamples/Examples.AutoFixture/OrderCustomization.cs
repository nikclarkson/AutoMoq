using AutoFixture;
using Orders;

namespace Examples.AutoFixture
{
    public class OrderCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
          //  fixture.Customize<Order>(c => c
                 //  .Without(x => x.OrderId));

          //  var specimen = fixture.Build<Order>()
      //      .OmitAutoProperties()
         //   .With(x => x.OrderId)
          //  .Create();

        //    fixture.Register(() => specimen);

            fixture.Customizations.Add(new OrderBuilder());
        }
    }
}
