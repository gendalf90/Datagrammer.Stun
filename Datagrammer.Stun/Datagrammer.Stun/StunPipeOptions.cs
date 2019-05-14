using Datagrammer.Middleware;

namespace Datagrammer.Stun
{
    public sealed class StunPipeOptions
    {
        public byte[] TransactionId { get; set; }

        public int ResponseBufferCapacity { get; set; } = 1;

        public MiddlewareOptions MiddlewareOptions { get; set; } = new MiddlewareOptions();
    }
}
