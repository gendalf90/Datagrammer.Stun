namespace Stun.Protocol
{
    public static class StunAttributeType
    {
        public const short MappedAddress = 0x0001;
        public const short ResponseAddress = 0x0002;
        public const short ChangeRequest = 0x0003;
        public const short SourceAddress = 0x0004;
        public const short ChangedAddress = 0x0005;
        public const short Username = 0x0006;
        public const short Password = 0x0007;
        public const short MessageIntegrity = 0x0008;
        public const short ErrorCode = 0x0009;
        public const short UnknownAttribute = 0x000A;
        public const short ReflectedFrom = 0x000B;
    }
}
