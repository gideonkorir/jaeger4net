﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Metrics
{
    public class Metric
    {
        readonly IStatsFactory factory = null;

        public Metric(IStatsFactory factory)
        {
            this.factory = factory;
            TraceStartedSampled = factory.Counter("traces", Tag.Of("state", "started"), Tag.Of("sampled", "y"));
            TraceStartedNotSampled = factory.Counter("traces", Tag.Of("state", "started"), Tag.Of("sampled", "n"));
            TracesJoinedSampled = factory.Counter("traces", Tag.Of("state", "joined"), Tag.Of("sampled", "y"));
            TracesJoinedNotSampled = factory.Counter("traces", Tag.Of("state", "joined"), Tag.Of("sampled", "n"));
            SpansStarted = factory.Counter("spans", Tag.Of("state", "started"), Tag.Of("group", "lifecycle"));
            SpansFinished = factory.Counter("spans", Tag.Of("state", "finished"), Tag.Of("group", "lifecycle"));
            SpansSampled = factory.Counter("spans", Tag.Of("group", "sampling"), Tag.Of("sampled", "y"));
            SpansNotSampled = factory.Counter("spans", Tag.Of("group", "sampling"), Tag.Of("sampled", "n"));
            DecodingErrors = factory.Counter("decoding-errors");
            ReporterSuccess = factory.Counter("reporter-spans", Tag.Of("state", "success"));
            ReporterFailure = factory.Counter("reporter-spans", Tag.Of("state", "failure"));
            ReporterDropped = factory.Counter("spans", Tag.Of("state", "dropped"));
            ReporterQueueLength = factory.Gauge("reporter-queue");
            SamplerRetrieved = factory.Counter("sampler", Tag.Of("state", "retrieved"));
            SamplerUpdated = factory.Counter("sampler", Tag.Of("state", "updated"));
            SamplerQueryFailure = factory.Counter("sampler", Tag.Of("state", "failure"), Tag.Of("phase", "query"));
            SamplerParsingFailure = factory.Counter("sampler", Tag.Of("state", "failure"), Tag.Of("phase", "parsing"));
            BaggageUpdateSuccess = factory.Counter("baggage-update", Tag.Of("result", "ok"));
            BaggageUpdateFailure = factory.Counter("baggage-update", Tag.Of("result", "err"));
            BaggageTruncate = factory.Counter("baggage-truncate");
            BaggageRestrictionsUpdateSuccess = factory.Counter("baggage-restrictions-update", Tag.Of("result", "ok"));
            BaggageRestrictionsUpdateFailure = factory.Counter("baggage-restrictions-update", Tag.Of("result", "err"));
        }

        /// <summary>
        /// Number of traces by this tracer as sampled
        /// </summary>
        public Counter TraceStartedSampled { get; }

        /// <summary>
        /// Number of traces started by this tracer as not sampled
        /// </summary>
        public Counter TraceStartedNotSampled { get; }

        /// <summary>
        /// Number of externally started sampled traces this tracer joined
        /// </summary>
        public Counter TracesJoinedSampled { get; }

        /// <summary>
        /// Number of externally started not-sampled traces this tracer joined
        /// </summary>
        public Counter TracesJoinedNotSampled { get; }

        /// <summary>
        /// Number of sampled spans started by this tracer
        /// </summary>
        public Counter SpansStarted { get; }

        /// <summary>
        /// Number of sampled spans started by this tracer
        /// </summary>
        public Counter SpansFinished { get; }

        /// <summary>
        /// Number of sampled spans started by this tracer
        /// </summary>
        public Counter SpansSampled { get; }

        /// <summary>
        /// Number of not-sampled spans started by this tracer
        /// </summary>
        public Counter SpansNotSampled { get; }

        /// <summary>
        /// Number of errors decoding tracing context
        /// </summary>
        public Counter DecodingErrors { get; }

        /// <summary>
        /// Number of spans successfully reported
        /// </summary>
        public Counter ReporterSuccess { get; }

        /// <summary>
        /// Number of spans in failed attempts to report
        /// </summary>
        public Counter ReporterFailure { get; }

        /// <summary>
        /// Number of spans dropped due to internal queue overflow
        /// </summary>
        public Counter ReporterDropped { get; }

        /// <summary>
        /// Current number of spans in the reporter queue
        /// </summary>
        public Gauge ReporterQueueLength { get; }

        /// <summary>
        /// Number of times the Sampler succeeded to retrieve samping strategy
        /// </summary>
        public Counter SamplerRetrieved { get; }

        /// <summary>
        /// Number of times the Sampler succeeded to retrieve and updateGauge sampling strategy
        /// </summary>
        public Counter SamplerUpdated { get; }

        /// <summary>
        /// Number of times the Sampler failed to retrieve the sampling strategy
        /// </summary>
        public Counter SamplerQueryFailure { get; }

        /// <summary>
        /// Number of times the Sampler failed to parse retrieved sampling strategy
        /// </summary>
        public Counter SamplerParsingFailure { get; }

        /// <summary>
        /// Number of times baggage was successfully written or updated on spans
        /// </summary>
        public Counter BaggageUpdateSuccess { get; }

        /// <summary>
        /// Number of times baggage failed to write or update on spans
        /// </summary>
        public Counter BaggageUpdateFailure { get; }

        /// <summary>
        /// Number of times baggage was truncated as per baggage restrictions
        /// </summary>
        public Counter BaggageTruncate { get; }

        /// <summary>
        /// Number of times baggage restrictions were successfully updated
        /// </summary>
        public Counter BaggageRestrictionsUpdateSuccess { get; }

        /// <summary>
        /// Number of times baggage restrictions failed to update
        /// </summary>
        public Counter BaggageRestrictionsUpdateFailure { get; }
    }
}
