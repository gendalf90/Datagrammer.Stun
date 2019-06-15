using System;

namespace Stun.Protocol
{
    public readonly struct StunAttributes
    {
        private readonly ReadOnlyMemory<byte> bytes;

        public StunAttributes(ReadOnlyMemory<byte> bytes)
        {
            this.bytes = bytes;
        }

        public StunAttributeEnumerator GetEnumerator()
        {
            return new StunAttributeEnumerator(bytes);
        }
    }
}
