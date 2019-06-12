using System;

namespace Datagrammer.Stun
{
    public readonly struct StunIPEndPoint
    {
        private const byte StunIPv4Family = 0x01;
        private const byte StunIPv6Family = 0x02;
        private const short StunIPv4EndPointLength = 8;
        private const short StunIPv6EndPointLength = 20;

        public StunIPEndPoint(ReadOnlyMemory<byte> address, int port)
        {
            Address = address;
            Port = port;
        }

        public ReadOnlyMemory<byte> Address { get; }

        public int Port { get; }

        public static bool TryParse(ReadOnlyMemory<byte> bytes, out StunIPEndPoint endPoint)
        {
            if (TryParseIPv4EndPoint(bytes, out endPoint))
            {
                return true;
            }

            if (TryParseIPv6EndPoint(bytes, out endPoint))
            {
                return true;
            }

            return false;
        }

        private static bool TryParseIPv4EndPoint(ReadOnlyMemory<byte> bytes, out StunIPEndPoint endPoint)
        {
            endPoint = new StunIPEndPoint();

            if (!IsIPv4(bytes))
            {
                return false;
            }

            endPoint = CreateEndPoint(bytes.Slice(4, 4), bytes.Slice(2, 2));
            return true;
        }

        private static bool IsIPv4(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Span[1] == StunIPv4Family && bytes.Length >= StunIPv4EndPointLength;
        }

        private static bool TryParseIPv6EndPoint(ReadOnlyMemory<byte> bytes, out StunIPEndPoint endPoint)
        {
            endPoint = new StunIPEndPoint();

            if (!IsIPv6(bytes))
            {
                return false;
            }

            endPoint = CreateEndPoint(bytes.Slice(4, 16), bytes.Slice(2, 2));
            return true;
        }

        private static bool IsIPv6(ReadOnlyMemory<byte> bytes)
        {
            return bytes.Span[1] == StunIPv6Family && bytes.Length >= StunIPv6EndPointLength;
        }

        private static StunIPEndPoint CreateEndPoint(ReadOnlyMemory<byte> address, ReadOnlyMemory<byte> port)
        {
            return new StunIPEndPoint(address, NetworkBitConverter.ToInt16(port.Span));
        }
    }
}
