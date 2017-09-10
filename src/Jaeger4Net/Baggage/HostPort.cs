using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Jaeger4Net.Baggage
{
    public struct HostPort
    {
        public static readonly HostPort DefaultHostPort = new HostPort("localhost", 5778);

        public string Host { get; }
        public int Port { get; }

        public HostPort(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public override string ToString()
            => $"{Host}:{Port}";
    }
}
