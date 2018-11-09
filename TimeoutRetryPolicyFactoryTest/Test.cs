using System;
using System.Net;
using System.Threading.Tasks;
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
        public async Task Failure()
        {
            var fatorySettings = new TimeoutRetryPolicySettings { PerRetryTimeout = "00:00:01", TotalTimeout = "00:00:03" };
            var factory = new TimeoutRetryPolicyFactory(fatorySettings);
            var policy = factory.Create();

            var badUrl = "http://10.51.11.50:15672/";
            var request = WebRequest.Create(badUrl);

            Func<Task> executeAction = async () => await policy.ExecuteAsync(() => request.GetResponseAsync());
            executeAction.Should().Throw<Exception>();
        }
    }
}
