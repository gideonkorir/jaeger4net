using Jaeger4Net.Sampling;
using Jaeger4Net.Utils;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jaeger4Net.Tests.Sampling
{
    public class PerOperationSamplerTests
    {
        readonly double SamplingRate = 0.31415,
            DefaultSamplingProbability = 0.512,
            DefaultLowerBoundTracesPerSecond = 2.0,
            Delta = 0.001;
        readonly int MaxOperations = 100;
        readonly long TraceId = 1L;
        const string Operation = "some OPERATION";
        readonly IClock clock = new CoreClrClock();

        [Fact]
        public void TestFallbackToDefaultProbabilisticSampler()
        {
            var fallback = new Mock<ProbabilisticSampler>(0.999);
            var sampler = new PerOperationSampler(
                0,
                new OperationSamplingParameters()
                {
                    PerOperationStrategies = new List<PerOperationSamplingParameters>(),
                    DefaultLowerBoundTracesPerSecond = 0, //ratelimiter will return false
                    DefaultSamplingProbability = 1 //always return true
                }, fallback.Object, clock
                );
            fallback.Setup(c => c.Sample(Operation, TraceId))
                .CallBase()
                .Verifiable();
            var status = sampler.Sample(Operation, TraceId);
            Assert.True(status);
            fallback.Verify();
        }

        [Fact]
        public void TestCreateGuaranteedSamplerOnUnseenOperation()
        {
            var newOp = "new Operation";
            
            var sampler = new PerOperationSampler(MaxOperations,
                new OperationSamplingParameters()
                {
                    DefaultLowerBoundTracesPerSecond = DefaultLowerBoundTracesPerSecond,
                    DefaultSamplingProbability = DefaultSamplingProbability,
                    PerOperationStrategies = new List<PerOperationSamplingParameters>()
                }, clock);

            Assert.False(sampler.OperationToSamplers.TryGetValue(newOp, out var _));
            Assert.True(sampler.Sample(newOp, TraceId));
            Assert.True(sampler.OperationToSamplers.TryGetValue(newOp, out var opSampler));
            Assert.Equal(new GuaranteedThroughputSampler(DefaultSamplingProbability, DefaultLowerBoundTracesPerSecond, clock), opSampler);
        }

        [Fact]
        public void TestPerOperationSamplerWithKnownOperation()
        {
            var fallback = new Mock<ProbabilisticSampler>(MockBehavior.Default, 0.0);

            var sampler = new PerOperationSampler(1,
                new OperationSamplingParameters()
                {
                    DefaultLowerBoundTracesPerSecond = 0,
                    DefaultSamplingProbability = 0,
                    PerOperationStrategies = new List<PerOperationSamplingParameters>()
                    {
                        new PerOperationSamplingParameters()
                        {
                            Operation = Operation,
                            ProbabilisticSampling = new ProbabilisticSamplingParameter()
                            {
                                SamplingRate = 0.999
                            }
                        }
                    }
                }, fallback.Object, clock);
            fallback.Setup(c => c.Sample(Operation, TraceId))
                .Verifiable();

            var status = sampler.Sample(Operation, TraceId);
            Assert.True(status);
            Assert.True(sampler.OperationToSamplers.TryGetValue(Operation, out var sampler2));

            fallback.Verify(c => c.Sample(Operation, TraceId), Times.Never);
        }
    }
}
