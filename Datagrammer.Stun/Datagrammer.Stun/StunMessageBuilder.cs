using System;

namespace Datagrammer.Stun
{
    public struct StunMessageBuilder
    {
        private const short StunMessageHeaderLength = 20;
        private const short StunAttributeHeaderLength = 4;
        private const int StunMagicCookie = 0x2112A442;

        private short type;
        private StunTransactionId transactionId;
        private ReadOnlyMemory<byte> attributes;

        public StunMessageBuilder SetType(short type)
        {
            this.type = type;
            return this;
        }

        public StunMessageBuilder SetTransactionId(StunTransactionId transactionId)
        {
            this.transactionId = transactionId;
            return this;
        }

        public StunMessageBuilder AddAttribute(StunAttribute attribute)
        {
            var bytes = new byte[attributes.Length + StunAttributeHeaderLength + attribute.Content.Length];

            CopyAttributesTo(bytes);
            WriteAttributeType(attribute.Type, SliceNewAttribute(bytes));
            WriteAttributeContentLength((short)attribute.Content.Length, SliceNewAttribute(bytes));
            WriteAttributeContent(attribute.Content.Span, SliceNewAttribute(bytes));

            attributes = bytes;
            return this;
        }

        private void CopyAttributesTo(Span<byte> bytes)
        {
            attributes.Span.CopyTo(bytes);
        }

        private Span<byte> SliceNewAttribute(Span<byte> bytes)
        {
            return bytes.Slice(attributes.Length);
        }

        private void WriteAttributeType(short attributeType, Span<byte> bytes)
        {
            NetworkBitConverter.WriteBytes(bytes.Slice(0, 2), attributeType);
        }

        private void WriteAttributeContentLength(short attributeContentLength, Span<byte> bytes)
        {
            NetworkBitConverter.WriteBytes(bytes.Slice(2, 2), attributeContentLength);
        }

        private void WriteAttributeContent(ReadOnlySpan<byte> attributeContent, Span<byte> bytes)
        {
            attributeContent.CopyTo(bytes.Slice(StunAttributeHeaderLength));
        }

        public ReadOnlyMemory<byte> Build()
        {
            var bytes = new byte[StunMessageHeaderLength + attributes.Length];

            WriteType(bytes);
            WriteMagicCookie(bytes);
            WriteTransactionId(bytes);
            WriteContentLength(bytes);
            WriteAttributes(bytes);

            return bytes;
        }

        private void WriteType(Span<byte> bytes)
        {
            NetworkBitConverter.WriteBytes(bytes.Slice(0, 2), type);
        }

        private void WriteMagicCookie(Span<byte> bytes)
        {
            UnsafeBitConverter.WriteBytes(bytes.Slice(4, 4), StunMagicCookie);
        }

        private void WriteTransactionId(Span<byte> bytes)
        {
            transactionId.AsMemory().Span.CopyTo(bytes.Slice(8, 12));
        }

        private void WriteContentLength(Span<byte> bytes)
        {
            NetworkBitConverter.WriteBytes(bytes.Slice(2, 2), (short)attributes.Length);
        }

        private void WriteAttributes(Span<byte> bytes)
        {
            attributes.Span.CopyTo(bytes.Slice(StunMessageHeaderLength));
        }
    }
}
