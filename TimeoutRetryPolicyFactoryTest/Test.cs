using System;
using FluentAssertions;
using Xunit;

namespace TimeoutRetryPolicyFactoryTest
{
    public class Test
    {
        [Fact]
        public void Can_Parse_20_Seconds_Timeout()
        {
            TimeSpan.Parse("00:00:20").TotalSeconds.Should().Be(20);
        }

        [Fact]
        public void Foo()
        {
        }
    }
}
