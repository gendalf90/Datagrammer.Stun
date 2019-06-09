using System;

namespace Datagrammer.Stun
{
    public readonly struct StunMessage
    {
        private const short StunMessageHeaderLength = 20;
        private const int StunMagicCookie = 0x2112A442;

        private readonly ReadOnlyMemory<byte> bytes;

        internal StunMessage(ReadOnlyMemory<byte> bytes)
        {
            Type = NetworkBitConverter.ToInt16(bytes.Span.Slice(0, 2));
            TransactionId = new StunTransactionId(bytes.Slice(8, 12));
            
            this.bytes = bytes;
        }

        public short Type { get; }

        public StunTransactionId TransactionId { get; }

        public StunAttributeEnumerator GetEnumerator()
        {
            return bytes.IsEmpty ? new StunAttributeEnumerator() : new StunAttributeEnumerator(SliceAttributes(bytes));
        }

        public static bool TryParse(ReadOnlyMemory<byte> bytes, out StunMessage message)
        {
            message = new StunMessage();

            if(!IsItStunMessage(bytes))
            {
                return false;
            }

            message = new StunMessage(bytes);
            return true;
        }

        private static ReadOnlyMemory<byte> SliceAttributes(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Slice(StunMessageHeaderLength, NetworkBitConverter.ToInt16(bytes.Span.Slice(2, 2)));
        }

        private static bool IsItStunMessage(ReadOnlyMemory<byte> bytes)
        {
            if (!IsHeaderLengthValid(bytes))
            {
                return false;
            }

            if (!HasMagicCookie(bytes))
            {
                return false;
            }

            if (!IsContentLengthValid(bytes))
            {
                return false;
            }

            return true;
        }

        private static bool IsHeaderLengthValid(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Length >= StunMessageHeaderLength;
        }

        private static bool HasMagicCookie(ReadOnlyMemory<byte> bytes)
        {
            return UnsafeBitConverter.ToInt32(bytes.Span.Slice(4, 4)) == StunMagicCookie;
        }

        private static bool IsContentLengthValid(ReadOnlyMemory<byte> bytes)
        {
            return StunMessageHeaderLength + NetworkBitConverter.ToInt16(bytes.Span.Slice(2, 2)) <= bytes.Length;
        }
    }
}
