using System;
using System.Net;

namespace Datagrammer.Stun
{
    public sealed class StunGeneratorOptions
    {
        public Guid TransactionId { get; set; }

        public IPEndPoint Server { get; set; }

        public TimeSpan MessageSendingPeriod { get; set; } = TimeSpan.FromMilliseconds(500);
    }
}
