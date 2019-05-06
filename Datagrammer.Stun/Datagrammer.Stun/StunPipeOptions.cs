using Datagrammer.Middleware;
using System;

namespace Datagrammer.Stun
{
    public sealed class StunPipeOptions
    {
        public Guid TransactionId { get; set; }

        public MiddlewareOptions MiddlewareOptions { get; set; } = new MiddlewareOptions();
    }
}
