namespace Stun.Protocol
{
    public readonly struct StunMappedAddressAttribute
    {
        public StunMappedAddressAttribute(StunIPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        public StunIPEndPoint EndPoint { get; }

        public static bool TryParse(StunAttribute attribute, out StunMappedAddressAttribute result)
        {
            result = new StunMappedAddressAttribute();

            if (attribute.Type != StunAttributeType.MappedAddress)
            {
                return false;
            }

            if (!StunIPEndPoint.TryParse(attribute.Content, out var stunEndPoint))
            {
                return false;
            }

            result = new StunMappedAddressAttribute(stunEndPoint);
            return true;
        }
    }
}
