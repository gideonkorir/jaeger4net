using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Sampling
{
    /// <summary>
    /// Introduced this so that I don't have to mock everything.
    /// Avoid Test Induced Damage
    /// </summary>
    interface ISamplerObserver
    {
        /// <summary>
        /// called when sampling rate is updated
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="operation"></param>
        void OnSamplingRateUpdated(ISampler sampler, double samplingRate);
        /// <summary>
        /// Called when lower bound is updated
        /// </summary>
        /// <param name="lowerBound"></param>
        /// <param name="operation"></param>
        void OnLowerBoundUpdated(ISampler sampler, double lowerBound);

        /// <summary>
        /// Called when sample.Sample is invoked
        /// </summary>
        /// <param name="sampler"></param>
        /// <param name="operation"></param>
        /// <param name="traceId"></param>
        /// <param name="outcome"></param>
        void OnSampled(ISampler sampler, string operation, long traceId, SamplingStatus outcome);
    }
}
