using System;

namespace Datagrammer.Stun
{
    public readonly struct StunAttribute
    {
        public StunAttribute(short type, ReadOnlyMemory<byte> content)
        {
            Type = type;
            Content = content;
        }

        public short Type { get; }

        public ReadOnlyMemory<byte> Content { get; }
    }
}
