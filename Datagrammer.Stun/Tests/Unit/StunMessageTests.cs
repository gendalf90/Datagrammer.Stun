using Stun.Protocol;
using System;
using Xunit;

namespace Tests.Unit
{
    public class StunMessageTests
    {
        [Fact]
        public void InitializeMessage()
        {
            var transactionId = StunTransactionId.Generate();
            var message = new StunBuilderStep().SetTransactionId(transactionId)
                                               .SetType(StunMessageType.BindingRequest)
                                               .Build();

            var result = StunMessage.TryParse(message, out var parsedMessage);

            Assert.True(result);
            Assert.Equal(StunMessageType.BindingRequest, parsedMessage.Type);
            Assert.Equal(transactionId, parsedMessage.TransactionId);
        }

        [Fact]
        public void InitializeAttributes()
        {
            var firstAttribute = new StunAttribute(StunAttributeType.ChangedAddress, new byte[] { 1, 2, 3 });
            var secondAttribute = new StunAttribute(StunAttributeType.MappedAddress, new byte[] { 4, 5, 6 });
            var message = new StunBuilderStep().AddAttribute(firstAttribute)
                                               .AddAttribute(secondAttribute)
                                               .Build();

            var result = StunMessage.TryParse(message, out var parsedMessage);
            var parsedAttributes = parsedMessage.Attributes.GetEnumerator();

            Assert.True(result);
            Assert.True(parsedAttributes.MoveNext());
            Assert.Equal(firstAttribute.Type, parsedAttributes.Current.Type);
            Assert.True(firstAttribute.Content.Span.SequenceEqual(parsedAttributes.Current.Content.Span));
            Assert.True(parsedAttributes.MoveNext());
            Assert.Equal(secondAttribute.Type, parsedAttributes.Current.Type);
            Assert.True(secondAttribute.Content.Span.SequenceEqual(parsedAttributes.Current.Content.Span));
        }

        [Fact]
        public void InitializeAttributeList()
        {
            var firstAttribute = new StunAttribute(StunAttributeType.ChangedAddress, new byte[] { 1, 2, 3 });
            var secondAttribute = new StunAttribute(StunAttributeType.MappedAddress, new byte[] { 4, 5, 6 });
            var message = new StunBuilderStep().AddAttributes(new StunAttribute[] { firstAttribute, secondAttribute })
                                               .Build();

            var result = StunMessage.TryParse(message, out var parsedMessage);
            var parsedAttributes = parsedMessage.Attributes.GetEnumerator();

            Assert.True(result);
            Assert.True(parsedAttributes.MoveNext());
            Assert.Equal(firstAttribute.Type, parsedAttributes.Current.Type);
            Assert.True(firstAttribute.Content.Span.SequenceEqual(parsedAttributes.Current.Content.Span));
            Assert.True(parsedAttributes.MoveNext());
            Assert.Equal(secondAttribute.Type, parsedAttributes.Current.Type);
            Assert.True(secondAttribute.Content.Span.SequenceEqual(parsedAttributes.Current.Content.Span));
        }
    }
}
