using System.Net;

namespace Stun.Protocol
{
    public static class StunExtensions
    {
        public static bool TryParseMappedAddress(this StunAttributes attributes, out IPEndPoint address)
        {
            address = null;

            foreach (var attribute in attributes)
            {
                if(TryParseMappedAddressAttribute(attribute, out address))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryParseMappedAddressAttribute(StunAttribute attribute, out IPEndPoint endPoint)
        {
            endPoint = null;

            if (attribute.Type != StunAttributeType.MappedAddress)
            {
                return false;
            }

            if(!StunIPEndPoint.TryParse(attribute.Content, out var stunEndPoint))
            {
                return false;
            }

            endPoint = new IPEndPoint(new IPAddress(stunEndPoint.Address.ToArray()), stunEndPoint.Port);
            return true;
        }
    }
}
