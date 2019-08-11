using System;

namespace Stun.Protocol
{
    public readonly struct StunTransactionId : IEquatable<StunTransactionId>
    {
        private const int TransactionIdLength = 12;

        private static readonly byte[] empty = new byte[TransactionIdLength];

        private readonly ReadOnlyMemory<byte> bytes;

        public StunTransactionId(ReadOnlyMemory<byte> bytes)
        {
            if(bytes.Length != TransactionIdLength)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), $"Transaction id must be with length {TransactionIdLength}");
            }

            this.bytes = bytes;
        }

        public ReadOnlyMemory<byte> AsMemory()
        {
            return bytes.IsEmpty ? empty : bytes;
        }

        public bool Equals(StunTransactionId other)
        {
            return AsMemory().Span.SequenceEqual(other.AsMemory().Span);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;

                foreach(var b in AsMemory().Span)
                {
                    hash = hash * 31 + b;
                }

                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if(obj is StunTransactionId id)
            {
                return Equals(id);
            }

            return false;
        }

        public static StunTransactionId Generate()
        {
            var bytes = new byte[TransactionIdLength];

            ThreadSafeRandom.NextBytes(bytes);

            return new StunTransactionId(bytes);
        }

        public static bool operator ==(StunTransactionId first, StunTransactionId second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(StunTransactionId first, StunTransactionId second)
        {
            return !first.Equals(second);
        }
    }
}
