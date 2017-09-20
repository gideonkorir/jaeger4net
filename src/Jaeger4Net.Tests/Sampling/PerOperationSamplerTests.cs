using Jaeger4Net.Sampling;
using Jaeger4Net.Utils;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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

        readonly OperationSamplingParameters defaultParameters;

        public PerOperationSamplerTests()
        {
            defaultParameters = new OperationSamplingParameters
            {
                DefaultLowerBoundTracesPerSecond = DefaultLowerBoundTracesPerSecond,
                DefaultSamplingProbability = DefaultSamplingProbability,
                PerOperationStrategies = new List<PerOperationSamplingParameters>()
                {
                    new PerOperationSamplingParameters()
                    {
                        Operation = Operation,
                        ProbabilisticSampling = SamplingRate
                    }
                }
            };
        }

        [Fact]
        public void TestFallbackToDefaultProbabilisticSampler()
        {
            var observer = new RecordingSamplingObserver();

            var sampler = new PerOperationSampler(
                0,
                new OperationSamplingParameters()
                {
                    PerOperationStrategies = new List<PerOperationSamplingParameters>(),
                    DefaultLowerBoundTracesPerSecond = 0, //ratelimiter will return false
                    DefaultSamplingProbability = 0.999 //always return true
                }, clock, observer
                );
            var status = sampler.Sample(Operation, TraceId);
            Assert.True(status);
            Assert.True(observer.SamplesByOperation.TryGetValue(Operation, out var info));
            Assert.Equal(TraceId, info.TraceId);
            Assert.True(info.Status);
        }

        [Fact]
        public void TestCreateGuaranteedSamplerOnUnseenOperation()
        {
            var newOp = "new Operation";
            //each test gets it's own copy. Yay xunit
            defaultParameters.PerOperationStrategies.Clear();
            
            var sampler = new PerOperationSampler(MaxOperations,
                defaultParameters, clock);

            Assert.False(sampler.OperationToSamplers.TryGetValue(newOp, out var _));
            Assert.True(sampler.Sample(newOp, TraceId));
            Assert.True(sampler.OperationToSamplers.TryGetValue(newOp, out var opSampler));
            Assert.Equal(new GuaranteedThroughputSampler(DefaultSamplingProbability, DefaultLowerBoundTracesPerSecond, clock), opSampler);
        }

        [Fact]
        public void TestPerOperationSamplerWithKnownOperation()
        {
            var observer = new RecordingSamplingObserver();

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
                }, clock, observer);

            var status = sampler.Sample(Operation, TraceId);
            Assert.True(status);
            Assert.True(sampler.OperationToSamplers.TryGetValue(Operation, out var sampler2));

            Assert.Single(observer.SamplesByOperation);
            var used = observer.SamplesByOperation[Operation].Sampler as ProbabilisticSampler;
            Assert.Equal(0.999, used.SamplingRate);
        }

        [Fact]
        public void TestUpdate()
        {
            var observer = new RecordingSamplingObserver();
            var constructorArgs = new OperationSamplingParameters
            {
                DefaultLowerBoundTracesPerSecond = 0.1,
                DefaultSamplingProbability = 0.1,
                PerOperationStrategies = new List<PerOperationSamplingParameters>()
                {
                    new PerOperationSamplingParameters()
                    {
                        Operation = Operation,
                        ProbabilisticSampling = 0.898
                    }
                }
            };
            var sampler = new PerOperationSampler(MaxOperations, constructorArgs, clock, observer);

            constructorArgs.PerOperationStrategies[0].ProbabilisticSampling = 0.999;

            Assert.True(sampler.Update(constructorArgs));
            Assert.Empty(observer.LowerBounds);
            Assert.Single(observer.SamplingRates);
            var cast = observer.SamplingRates.First().Key as ProbabilisticSampler;
            Assert.Equal(0.999, cast.SamplingRate);

        }

        [Fact]
        public void TestNoUpdate()
        {
            var observer = new RecordingSamplingObserver();
            var constructorArgs = new OperationSamplingParameters
            {
                DefaultLowerBoundTracesPerSecond = 0.1,
                DefaultSamplingProbability = 0.1,
                PerOperationStrategies = new List<PerOperationSamplingParameters>()
                {
                    new PerOperationSamplingParameters()
                    {
                        Operation = Operation,
                        ProbabilisticSampling = 0.898
                    }
                }
            };
            var sampler = new PerOperationSampler(MaxOperations, constructorArgs, clock, observer);

            Assert.False(sampler.Update(constructorArgs));
            Assert.Empty(observer.SamplingRates);
            Assert.Empty(observer.LowerBounds);
        }

        [Fact]
        public void TestUpdateIgnoredGreaterThanMax()
        {
            var observer = new RecordingSamplingObserver();
            var constructorArgs = new OperationSamplingParameters
            {
                DefaultLowerBoundTracesPerSecond = 0.1,
                DefaultSamplingProbability = 0.1,
                PerOperationStrategies = new List<PerOperationSamplingParameters>()
                {
                    new PerOperationSamplingParameters()
                    {
                        Operation = Operation,
                        ProbabilisticSampling = 0.898
                    }
                }
            };
            //max 1 operation
            var sampler = new PerOperationSampler(1, constructorArgs, clock, observer);

            //add a new operation
            constructorArgs.PerOperationStrategies.Add(new PerOperationSamplingParameters()
            {
                Operation = "new",
                ProbabilisticSampling = 0.54
            });


            Assert.False(sampler.Update(constructorArgs));
            Assert.Empty(observer.SamplingRates);
            Assert.Empty(observer.LowerBounds);
        }

        [Fact]
        public void TestUpdateAddOperation()
        {
            var observer = new RecordingSamplingObserver();
            var constructorArgs = new OperationSamplingParameters
            {
                DefaultLowerBoundTracesPerSecond = 0.1,
                DefaultSamplingProbability = 0.1,
                PerOperationStrategies = new List<PerOperationSamplingParameters>()
                {
                    new PerOperationSamplingParameters()
                    {
                        Operation = Operation,
                        ProbabilisticSampling = 0.898
                    }
                }
            };
            //max 1 operation
            var sampler = new PerOperationSampler(2, constructorArgs, clock, observer);

            //add a new operation
            constructorArgs.PerOperationStrategies.Add(new PerOperationSamplingParameters()
            {
                Operation = "new",
                ProbabilisticSampling = 0.54
            });

            Assert.Single(sampler.OperationToSamplers);

            Assert.True(sampler.Update(constructorArgs));
            Assert.Empty(observer.SamplingRates);
            Assert.Empty(observer.LowerBounds);

            Assert.Equal(2, sampler.OperationToSamplers.Count);
            Assert.True(sampler.OperationToSamplers.TryGetValue("new", out var _));
        }
    }


}
