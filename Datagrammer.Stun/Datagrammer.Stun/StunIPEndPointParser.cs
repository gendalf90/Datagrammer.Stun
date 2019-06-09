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

            if (!IsIPv4(bytes))
            {
                return false;
            }

            endPoint = CreateEndPoint(bytes.Slice(4, 4), bytes.Slice(2, 2));
            return true;
        }

        private static bool IsIPv4(ReadOnlySpan<byte> bytes)
        {
            return bytes[1] == StunIPv4Family && bytes.Length >= StunIPv4EndPointLength;
        }

        private static bool TryParseIPv6EndPoint(ReadOnlySpan<byte> bytes, out IPEndPoint endPoint)
        {
            endPoint = null;

            if (!IsIPv6(bytes))
            {
                return false;
            }

            endPoint = CreateEndPoint(bytes.Slice(4, 16), bytes.Slice(2, 2));
            return true;
        }

        private static bool IsIPv6(ReadOnlySpan<byte> bytes)
        {
            return bytes[1] == StunIPv6Family && bytes.Length >= StunIPv6EndPointLength;
        }

        private static IPEndPoint CreateEndPoint(ReadOnlySpan<byte> address, ReadOnlySpan<byte> port)
        {
            return new IPEndPoint(new IPAddress(address.ToArray()), NetworkBitConverter.ToInt16(port));
        }
    }
}
