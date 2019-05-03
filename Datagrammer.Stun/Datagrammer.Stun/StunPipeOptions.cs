using Datagrammer.Middleware;
using System;

namespace Datagrammer.Stun
{
    public sealed class StunPipeOptions
    {
        public Guid TransactionId { get; set; } = Guid.Empty;

        public int ResponseBufferCapacity { get; set; } = 1;

        public MiddlewareOptions MiddlewareOptions { get; set; } = new MiddlewareOptions();
    }
}
