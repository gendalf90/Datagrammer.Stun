using System;
using System.Net;

namespace Datagrammer.Stun
{
    public sealed class StunGeneratorOptions
    {
        public Guid TransactionId { get; set; } = Guid.Empty;

        public IPEndPoint[] Servers { get; set; } = Array.Empty<IPEndPoint>();

        public TimeSpan MessageSendingPeriod { get; set; } = TimeSpan.FromMilliseconds(500);
    }
}
