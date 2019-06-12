﻿using System;

namespace Datagrammer.Stun
{
    public struct StunAttributeEnumerator
    {
        private const short StunAttributeHeaderLength = 4;

        private ReadOnlyMemory<byte> remainsOfAttributeBytes;
        private StunAttribute? currentAttribute;
        private short currentAttributeContentLength;

        public StunAttributeEnumerator(ReadOnlyMemory<byte> bytes)
        {
            remainsOfAttributeBytes = bytes;
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
            get => remainsOfAttributeBytes.Length >= StunAttributeHeaderLength;
        }

        private void ReadAttributeContentLength()
        {
            currentAttributeContentLength = NetworkBitConverter.ToInt16(remainsOfAttributeBytes.Span.Slice(2, 2));
        }

        private bool IsAttributeContentLengthValid
        {
            get => StunAttributeHeaderLength + currentAttributeContentLength <= remainsOfAttributeBytes.Length;
        }

        private void ParseCurrentAttribute()
        {
            var type = NetworkBitConverter.ToInt16(remainsOfAttributeBytes.Span.Slice(0, 2));
            var contentBytes = remainsOfAttributeBytes.Slice(StunAttributeHeaderLength, currentAttributeContentLength);

            currentAttribute = new StunAttribute(type, contentBytes);
        }

        private void SliceRemainAttributes()
        {
            remainsOfAttributeBytes = remainsOfAttributeBytes.Slice(StunAttributeHeaderLength + currentAttributeContentLength);
        }
    }
}
