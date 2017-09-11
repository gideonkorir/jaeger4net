using Jaeger4Net.Baggage;
using Jaeger4Net.Metrics;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jaeger4Net.Tests.Baggage
{
    public class RemoteBaggageRestrictorTests
    {
        [Fact]
        public async Task On_Initialization_Failure_Option_Is_Respected_True()
        {
            var mock = new Mock<IRestrictionSource>();
            mock.Setup(c => c.FetchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<NotImplementedException>();

            var restrictor = new RemoteBaggageRestrictor(mock.Object, new RemoteRestrictorOptions()
            {
                DenyBaggageOnInitializationFailure = true
            }, ClientMetrics.Null);
            await Task.Delay(1000); //give time for initialization
            var restriction = restrictor.Get("service", "key");
            Assert.Equal(Restriction.Invalid, restriction);
        }

        [Fact]
        public async Task On_Initialization_Failure_Option_Is_Respected_False()
        {
            var mock = new Mock<IRestrictionSource>();
            mock.Setup(c => c.FetchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws<NotImplementedException>();

            var restrictor = new RemoteBaggageRestrictor(mock.Object, new RemoteRestrictorOptions()
            {
                DenyBaggageOnInitializationFailure = false
            }, ClientMetrics.Null);
            await Task.Delay(1000); //give time for initialization
            var restriction = restrictor.Get("service", "key");
            Assert.Equal(Restriction.Valid, restriction);
        }

        [Fact]
        public async Task BaggageKey_Restricted_If_Key_Not_Found()
        {
            var mock = new Mock<IRestrictionSource>();
            IList<RestrictionResponse> restrictions = new List<RestrictionResponse>()
            {
                new RestrictionResponse("key", 245)
            };

            mock.Setup(c => c.FetchAsync("service", It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(restrictions));
            mock.Setup(c => c.FetchAsync("abc", It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IList<RestrictionResponse>>(new List<RestrictionResponse>()));


            var restrictor = new RemoteBaggageRestrictor(mock.Object, new RemoteRestrictorOptions()
            {
                Services = new[] { "service" },
                DenyBaggageOnInitializationFailure = true
            }, ClientMetrics.Null);
            await Task.Delay(1000); //give time for initialization
            var restriction = restrictor.Get("service", "key");

            Assert.Equal(new Restriction(true, 245), restriction);
            restriction = restrictor.Get("abc", "abc");
            Assert.Equal(Restriction.Invalid, restriction);
        }
    }
}
