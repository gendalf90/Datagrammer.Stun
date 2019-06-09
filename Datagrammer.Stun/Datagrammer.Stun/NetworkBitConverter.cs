using System;
using System.Net;

namespace Datagrammer.Stun
{
    internal static class NetworkBitConverter
    {
        public static short ToInt16(ReadOnlySpan<byte> bytes)
        {
            return IPAddress.NetworkToHostOrder(UnsafeBitConverter.ToInt16(bytes));
        }

        public static int ToInt32(ReadOnlySpan<byte> bytes)
        {
            return IPAddress.NetworkToHostOrder(UnsafeBitConverter.ToInt32(bytes));
        }

        public static void WriteBytes(Span<byte> destination, short value)
        {
            UnsafeBitConverter.WriteBytes(destination, IPAddress.HostToNetworkOrder(value));
        }

        public static void WriteBytes(Span<byte> destination, int value)
        {
            UnsafeBitConverter.WriteBytes(destination, IPAddress.HostToNetworkOrder(value));
        }
    }
}
