using System;

namespace Datagrammer.Stun
{
    public struct StunAttributeEnumerator
    {
        private const short StunAttributeHeaderLength = 4;

        private ReadOnlyMemory<byte> remainAttributeBytes;
        private StunAttribute? currentAttribute;
        private short currentAttributeContentLength;

        public StunAttributeEnumerator(ReadOnlyMemory<byte> bytes)
        {
            remainAttributeBytes = bytes;
            currentAttribute = null;
            currentAttributeContentLength = 0;
        }

        public StunAttribute Current => currentAttribute ?? throw new ArgumentOutOfRangeException(nameof(Current));

        public bool MoveNext()
        {
            ClearCurrentAttribute();

            if (!IsAttributeHeaderLengthValid)
            {
                return false;
            }

            ReadAttributeContentLength();

            if (!IsAttributeContentLengthValid)
            {
                return false;
            }

            ParseCurrentAttribute();
            SliceRemainAttributes();

            return true;
        }

        private void ClearCurrentAttribute()
        {
            currentAttribute = null;
            currentAttributeContentLength = 0;
        }

        private bool IsAttributeHeaderLengthValid
        {
            get => remainAttributeBytes.Length >= StunAttributeHeaderLength;
        }

        private void ReadAttributeContentLength()
        {
            currentAttributeContentLength = NetworkBitConverter.ToInt16(remainAttributeBytes.Span.Slice(2, 2));
        }

        private bool IsAttributeContentLengthValid
        {
            get => StunAttributeHeaderLength + currentAttributeContentLength <= remainAttributeBytes.Length;
        }

        private void ParseCurrentAttribute()
        {
            var type = NetworkBitConverter.ToInt16(remainAttributeBytes.Span.Slice(0, 2));
            var contentBytes = remainAttributeBytes.Slice(StunAttributeHeaderLength, currentAttributeContentLength);

            currentAttribute = new StunAttribute(type, contentBytes);
        }

        private void SliceRemainAttributes()
        {
            remainAttributeBytes = remainAttributeBytes.Slice(StunAttributeHeaderLength + currentAttributeContentLength);
        }
    }
}
