using Stun.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Xunit;

namespace Tests.Unit
{
    public class StunIpEndPointTests
    {
        [Fact]
        public void ParseValidEndPoint()
        {
            ushort port = 51000;
            port = (ushort)IPAddress.HostToNetworkOrder((short)port);
            var ip = IPAddress.Parse("192.168.1.1");
            var endPoint = new List<byte>();
            endPoint.Add(0);
            endPoint.Add(0x01);
            endPoint.AddRange(BitConverter.GetBytes(port));
            endPoint.AddRange(ip.GetAddressBytes());

            var result = StunIPEndPoint.TryParse(endPoint.ToArray(), out var parsedEndPoint);

            Assert.True(result);
            Assert.Equal(51000, parsedEndPoint.Port);
            Assert.True(ip.GetAddressBytes().SequenceEqual(parsedEndPoint.Address.ToArray()));
        }
    }
}
