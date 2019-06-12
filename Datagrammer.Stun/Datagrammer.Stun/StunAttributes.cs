using System;

namespace Datagrammer.Stun
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
