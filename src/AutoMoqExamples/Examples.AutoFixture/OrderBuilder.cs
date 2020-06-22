using AutoFixture.Kernel;
using Orders;
using System;
using System.Reflection;

namespace Examples.AutoFixture
{
    public class OrderBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            var expectedProperty = typeof(Order).GetProperty(nameof(Order.OrderId));

            if (expectedProperty.Equals(request))
            {
                return Guid.NewGuid().ToString();
            }

            return new NoSpecimen();
        }
  
    }
}
