using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net
{
    public class Constants
    {
        // TODO these should be configurable
        public const string X_UBER_SOURCE = "x-uber-source";

        /**
         * Span tag key to describe the type of sampler used on the root span.
         */
        public const string SAMPLER_TYPE_TAG_KEY = "sampler.type";

        /**
         * Span tag key to describe the parameter of the sampler used on the root span.
         */
        public const string SAMPLER_PARAM_TAG_KEY = "sampler.param";

        /**
         * The name of HTTP header or a TextMap carrier key which, if found in the carrier, forces the
         * trace to be sampled as "debug" trace. The value of the header is recorded as the tag on the
         * root span, so that the trace can be found in the UI using this value as a correlation ID.
         */
        public const string DEBUG_ID_HEADER_KEY = "jaeger-debug-id";

        /**
         * The name of the tag used to report client version.
         */
        public const string JAEGER_CLIENT_VERSION_TAG_KEY = "jaeger.version";

        /**
         * The name used to report host name of the process.
         */
        public const string TRACER_HOSTNAME_TAG_KEY = "hostname";

        /**
         * The name used to report ip of the process.
         */
        public const string TRACER_IP_TAG_KEY = "ip";
    }

}
