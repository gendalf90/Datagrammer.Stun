using System;
using System.Net;

namespace Datagrammer.Stun
{
    public static class NetworkBitConverter
    {
        public static short ToInt16(ReadOnlySpan<byte> bytes)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes));
        }

        public static int ToInt32(ReadOnlySpan<byte> bytes)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes));
        }

        public static long ToInt64(ReadOnlySpan<byte> bytes)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(bytes));
        }

        public static bool TryWriteBytes(Span<byte> destination, short value)
        {
            return BitConverter.TryWriteBytes(destination, IPAddress.HostToNetworkOrder(value));
        }

        public static bool TryWriteBytes(Span<byte> destination, int value)
        {
            return BitConverter.TryWriteBytes(destination, IPAddress.HostToNetworkOrder(value));
        }

        public static bool TryWriteBytes(Span<byte> destination, long value)
        {
            return BitConverter.TryWriteBytes(destination, IPAddress.HostToNetworkOrder(value));
        }
    }
}
