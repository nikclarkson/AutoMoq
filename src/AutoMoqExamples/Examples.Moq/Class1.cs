using FluentAssertions;
using System;
using Xunit;

namespace Examples.Moq
{
    public class Class1
    {
        [Fact]
        public void yo()
        {
            true.Should().BeTrue();
        }
    }
}
