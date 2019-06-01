namespace Datagrammer.Stun
{
    public static class StunMessageType
    {
        public const short BindingRequest = 0x0001;
        public const short BindingResponse = 0x0101;
        public const short BindingErrorResponse = 0x0111;
        public const short SharedSecretRequest = 0x0002;
        public const short SharedSecretResponse = 0x0102;
        public const short SharedSecretErrorResponse = 0x0112;
    }
}
