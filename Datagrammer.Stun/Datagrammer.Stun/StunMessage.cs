using System;
using System.Net;

namespace Datagrammer.Stun
{
    public sealed class StunMessage
    {
        private const byte StunIPv4Family = 0x01;
        private const byte StunIPv6Family = 0x02;
        private const short StunMessageHeaderLength = 20;
        private const short StunAttributeHeaderLength = 4;
        private const short StunMappedAddressAttributeType = 0x0001;
        private const short StunBindingResponseType = 0x0101;
        private const short StunBindingRequestType = 0x0001;
        private const short StunIPv4EndPointLength = 8;
        private const short StunIPv6EndPointLength = 20;
        private const int StunMagicCookie = 0x2112A442;

        private readonly StunTransactionId transactionId;

        public StunMessage(StunTransactionId transactionId)
        {
            this.transactionId = transactionId;
        }

        public bool TryParseBindingResponse(ReadOnlyMemory<byte> bytes, out StunResponse response)
        {
            response = null;

            if(!IsItStunMessage(bytes))
            {
                return false;
            }

            if (!IsBindingResponseType(bytes))
            {
                return false;
            }

            if (!HasCurrentTransactionId(bytes))
            {
                return false;
            }

            response = new StunResponse();

            ParseAttributes(SliceMessageHeader(bytes), response);

            return true;
        }

        private ReadOnlyMemory<byte> SliceMessageHeader(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Slice(StunMessageHeaderLength);
        }

        private bool IsItStunMessage(ReadOnlyMemory<byte> bytes)
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

        private void ParseAttributes(ReadOnlyMemory<byte> bytes, StunResponse context)
        {
            if(!IsAttributeHeaderLengthValid(bytes))
            {
                return;
            }

            if (!IsAttributeContentLengthValid(bytes))
            {
                return;
            }

            ParseMappedAddressAttribute(bytes, context);

            ParseAttributes(SliceCurrentAttribute(bytes), context);
        }

        private ReadOnlyMemory<byte> SliceCurrentAttribute(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Slice(StunAttributeHeaderLength + NetworkBitConverter.ToInt16(bytes.Span.Slice(2, 2)));
        }

        private void ParseMappedAddressAttribute(ReadOnlyMemory<byte> bytes, StunResponse context)
        {
            if(!IsMappedAddressAttributeType(bytes))
            {
                return;
            }

            if(TryParseIPv4EndPoint(SliceAttributeHeader(bytes), out IPEndPoint ipv4EndPoint))
            {
                context.PublicAddress = ipv4EndPoint;
            }
            else if(TryParseIPv6EndPoint(SliceAttributeHeader(bytes), out IPEndPoint ipv6EndPoint))
            {
                context.PublicAddress = ipv6EndPoint;
            }
        }

        private ReadOnlyMemory<byte> SliceAttributeHeader(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Slice(StunAttributeHeaderLength);
        }

        private bool TryParseIPv4EndPoint(ReadOnlyMemory<byte> bytes, out IPEndPoint endPoint)
        {
            endPoint = null;

            if(!IsIPv4EndPointLengthValid(bytes))
            {
                return false;
            }

            if(!IsIPv4Family(bytes))
            {
                return false;
            }

            endPoint = new IPEndPoint(new IPAddress(bytes.Span.Slice(4, 4)), NetworkBitConverter.ToInt16(bytes.Span.Slice(2, 2)));
            return true;
        }

        private bool IsIPv4EndPointLengthValid(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Length >= StunIPv4EndPointLength;
        }

        private bool IsIPv4Family(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Span[1] == StunIPv4Family;
        }

        private bool TryParseIPv6EndPoint(ReadOnlyMemory<byte> bytes, out IPEndPoint endPoint)
        {
            endPoint = null;

            if (!IsIPv6EndPointLengthValid(bytes))
            {
                return false;
            }

            if (!IsIPv6Family(bytes))
            {
                return false;
            }

            endPoint = new IPEndPoint(new IPAddress(bytes.Span.Slice(4, 16)), NetworkBitConverter.ToInt16(bytes.Span.Slice(2, 2)));
            return true;
        }

        private bool IsIPv6EndPointLengthValid(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Length >= StunIPv6EndPointLength;
        }

        private bool IsIPv6Family(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Span[1] == StunIPv6Family;
        }

        private bool IsAttributeHeaderLengthValid(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Length >= StunAttributeHeaderLength;
        }

        private bool IsAttributeContentLengthValid(ReadOnlyMemory<byte> bytes)
        {
            return StunAttributeHeaderLength + NetworkBitConverter.ToInt16(bytes.Span.Slice(2, 2)) <= bytes.Length;
        }

        private bool IsMappedAddressAttributeType(ReadOnlyMemory<byte> bytes)
        {
            return NetworkBitConverter.ToInt16(bytes.Span.Slice(0, 2)) == StunMappedAddressAttributeType;
        }

        private bool IsHeaderLengthValid(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Length >= StunMessageHeaderLength;
        }

        private bool HasMagicCookie(ReadOnlyMemory<byte> bytes)
        {
            return BitConverter.ToInt32(bytes.Span.Slice(4, 4)) == StunMagicCookie;
        }

        private bool HasCurrentTransactionId(ReadOnlyMemory<byte> bytes)
        {
            return transactionId.Equals(new StunTransactionId(bytes.Slice(8, 12)));
        }

        private bool IsBindingResponseType(ReadOnlyMemory<byte> bytes)
        {
            return NetworkBitConverter.ToInt16(bytes.Span.Slice(0, 2)) == StunBindingResponseType;
        }

        private bool IsContentLengthValid(ReadOnlyMemory<byte> bytes)
        {
            return StunMessageHeaderLength + NetworkBitConverter.ToInt16(bytes.Span.Slice(2, 2)) <= bytes.Length;
        }

        public ReadOnlyMemory<byte> CreateBindingRequest()
        {
            var bytes = new byte[StunMessageHeaderLength];

            WriteBindingRequestType(bytes);
            WriteMagicCookie(bytes);
            WriteTransactionId(bytes);

            return bytes;
        }

        private void WriteBindingRequestType(Span<byte> bytes)
        {
            NetworkBitConverter.TryWriteBytes(bytes.Slice(0, 2), StunBindingRequestType);
        }

        private void WriteMagicCookie(Span<byte> bytes)
        {
            BitConverter.TryWriteBytes(bytes.Slice(4, 4), StunMagicCookie);
        }

        private void WriteTransactionId(Span<byte> bytes)
        {
            transactionId.AsMemory().Span.CopyTo(bytes.Slice(8, 12));
        }
    }
}
