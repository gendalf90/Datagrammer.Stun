using System;

namespace Stun.Protocol
{
    public readonly struct StunMessageType : IEquatable<StunMessageType>
    {
        private const short MaxStunType = 16383;

        private readonly ushort type;

        public StunMessageType(ushort type)
        {
            if (type > MaxStunType)
            {
                throw new ArgumentOutOfRangeException(nameof(type), $"Must be in range from 0 to {MaxStunType}");
            }

            this.type = type;
        }

        public bool Equals(StunMessageType other)
        {
            return type == other.type;
        }

        public override bool Equals(object obj)
        {
            if (obj is StunMessageType type)
            {
                return Equals(type);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return type;
        }

        public static implicit operator ushort(StunMessageType value)
        {
            return value.type;
        }

        public static StunMessageType BindingRequest => new StunMessageType(0x0001);

        public static StunMessageType BindingResponse => new StunMessageType(0x0101);

        public static StunMessageType BindingErrorResponse => new StunMessageType(0x0111);

        public static StunMessageType SharedSecretRequest => new StunMessageType(0x0002);

        public static StunMessageType SharedSecretResponse => new StunMessageType(0x0102);

        public static StunMessageType SharedSecretErrorResponse => new StunMessageType(0x0112);
    }
}
