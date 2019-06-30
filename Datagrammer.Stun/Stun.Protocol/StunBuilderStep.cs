using System;

namespace Stun.Protocol
{
    public readonly struct StunBuilderStep
    {
        private const short StunMessageHeaderLength = 20;
        private const short StunAttributeHeaderLength = 4;
        private const int StunMagicCookie = 0x2112A442;

        private readonly short type;
        private readonly StunTransactionId transactionId;
        private readonly ReadOnlyMemory<byte> attributes;

        internal StunBuilderStep(short type, StunTransactionId transactionId, ReadOnlyMemory<byte> attributes)
        {
            this.type = type;
            this.transactionId = transactionId;
            this.attributes = attributes;
        }

        public StunBuilderStep SetType(short type)
        {
            return new StunBuilderStep(type, transactionId, attributes);
        }

        public StunBuilderStep SetTransactionId(StunTransactionId transactionId)
        {
            return new StunBuilderStep(type, transactionId, attributes);
        }

        public StunBuilderStep AddAttributes(StunAttribute[] attributesToAdd)
        {
            var newAttributesLength = attributes.Length;

            for(int i = 0; i < attributesToAdd.Length; i++)
            {
                newAttributesLength += StunAttributeHeaderLength + attributesToAdd[i].Content.Length;
            }

            var newAttributeBytes = new byte[newAttributesLength];

            CopyCurrentAttributesTo(newAttributeBytes);

            var remainsOfNewAttributeBytes = SliceNewAttributes(newAttributeBytes);

            for(int i = 0; i < attributesToAdd.Length; i++)
            {
                remainsOfNewAttributeBytes = remainsOfNewAttributeBytes.Slice(WriteAttributeTo(attributesToAdd[i], remainsOfNewAttributeBytes));
            }

            return new StunBuilderStep(type, transactionId, newAttributeBytes);
        }

        public StunBuilderStep AddAttribute(StunAttribute attribute)
        {
            var newAttributeBytes = new byte[attributes.Length + StunAttributeHeaderLength + attribute.Content.Length];

            CopyCurrentAttributesTo(newAttributeBytes);

            WriteAttributeTo(attribute, SliceNewAttributes(newAttributeBytes));

            return new StunBuilderStep(type, transactionId, newAttributeBytes);
        }

        private int WriteAttributeTo(StunAttribute attribute, Span<byte> bytes)
        {
            WriteAttributeType(attribute.Type, bytes);
            WriteAttributeContentLength(attribute.Content.Length, bytes);
            WriteAttributeContent(attribute.Content.Span, bytes);

            return StunAttributeHeaderLength + attribute.Content.Length;
        }

        private void CopyCurrentAttributesTo(Span<byte> bytes)
        {
            attributes.Span.CopyTo(bytes);
        }

        private Span<byte> SliceNewAttributes(Span<byte> bytes)
        {
            return bytes.Slice(attributes.Length);
        }

        private void WriteAttributeType(short attributeType, Span<byte> bytes)
        {
            NetworkBitConverter.WriteBytes(bytes.Slice(0, 2), attributeType);
        }

        private void WriteAttributeContentLength(int attributeContentLength, Span<byte> bytes)
        {
            if(attributeContentLength > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException("Attribute length is too long");
            }

            NetworkBitConverter.WriteBytes(bytes.Slice(2, 2), (ushort)attributeContentLength);
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
            if(attributes.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException("Content length is too long");
            }

            NetworkBitConverter.WriteBytes(bytes.Slice(2, 2), (ushort)attributes.Length);
        }

        private void WriteAttributes(Span<byte> bytes)
        {
            attributes.Span.CopyTo(bytes.Slice(StunMessageHeaderLength));
        }
    }
}
