using System;
using System.Net;

namespace Datagrammer.Stun
{
    public static class StunIPEndPointParser
    {
        private const byte StunIPv4Family = 0x01;
        private const byte StunIPv6Family = 0x02;
        private const short StunIPv4EndPointLength = 8;
        private const short StunIPv6EndPointLength = 20;

        public static bool TryParse(ReadOnlySpan<byte> bytes, out IPEndPoint endPoint)
        {
            if(TryParseIPv4EndPoint(bytes, out endPoint))
            {
                return true;
            }

            if(TryParseIPv6EndPoint(bytes, out endPoint))
            {
                return true;
            }

            return false;
        }

        private static bool TryParseIPv4EndPoint(ReadOnlySpan<byte> bytes, out IPEndPoint endPoint)
        {
            endPoint = null;

            if (!IsIPv4EndPointLengthValid(bytes))
            {
                return false;
            }

            if (!IsIPv4Family(bytes))
            {
                return false;
            }

            endPoint = new IPEndPoint(new IPAddress(bytes.Slice(4, 4)), NetworkBitConverter.ToInt16(bytes.Slice(2, 2)));
            return true;
        }

        private static bool IsIPv4EndPointLengthValid(ReadOnlySpan<byte> bytes)
        {
            return bytes.Length >= StunIPv4EndPointLength;
        }

        private static bool IsIPv4Family(ReadOnlySpan<byte> bytes)
        {
            return bytes[1] == StunIPv4Family;
        }

        private static bool TryParseIPv6EndPoint(ReadOnlySpan<byte> bytes, out IPEndPoint endPoint)
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

            endPoint = new IPEndPoint(new IPAddress(bytes.Slice(4, 16)), NetworkBitConverter.ToInt16(bytes.Slice(2, 2)));
            return true;
        }

        private static bool IsIPv6EndPointLengthValid(ReadOnlySpan<byte> bytes)
        {
            return bytes.Length >= StunIPv6EndPointLength;
        }

        private static bool IsIPv6Family(ReadOnlySpan<byte> bytes)
        {
            return bytes[1] == StunIPv6Family;
        }
    }
}
