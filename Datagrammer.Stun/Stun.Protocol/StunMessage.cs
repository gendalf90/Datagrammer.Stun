using System;

namespace Stun.Protocol
{
    public readonly struct StunMessage
    {
        private const byte MaxFirstByteValue = 63;
        private const short StunMessageHeaderLength = 20;
        private const int StunMagicCookie = 0x2112A442;

        private StunMessage(ReadOnlyMemory<byte> bytes)
        {
            Type = new StunMessageType(NetworkBitConverter.ToUInt16(bytes.Span.Slice(0, 2)));
            TransactionId = new StunTransactionId(bytes.Slice(8, 12));
            Attributes = new StunAttributes(SliceAttributes(bytes));
        }

        public StunMessageType Type { get; }

        public StunTransactionId TransactionId { get; }

        public StunAttributes Attributes { get; }

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
            return bytes.Slice(StunMessageHeaderLength, NetworkBitConverter.ToUInt16(bytes.Span.Slice(2, 2)));
        }

        private static bool IsItStunMessage(ReadOnlyMemory<byte> bytes)
        {
            if (!IsHeaderLengthValid(bytes))
            {
                return false;
            }

            if (!HasFirstTwoBitsAreEmpty(bytes))
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

        private static bool HasFirstTwoBitsAreEmpty(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Span[0] <= MaxFirstByteValue;
        }

        private static bool HasMagicCookie(ReadOnlyMemory<byte> bytes)
        {
            return UnsafeBitConverter.ToInt32(bytes.Span.Slice(4, 4)) == StunMagicCookie;
        }

        private static bool IsContentLengthValid(ReadOnlyMemory<byte> bytes)
        {
            return StunMessageHeaderLength + NetworkBitConverter.ToUInt16(bytes.Span.Slice(2, 2)) <= bytes.Length;
        }
    }
}
