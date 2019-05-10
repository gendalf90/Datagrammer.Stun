using Datagrammer.Middleware;
using STUN;
using STUN.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Datagrammer.Stun
{
    public sealed class StunPipeBlock : MiddlewareBlock<Datagram, Datagram>
    {
        private readonly StunPipeOptions options;
        private readonly Func<StunResponse, Task> stunResponseHandler;

        public StunPipeBlock(Func<StunResponse, Task> stunResponseHandler, StunPipeOptions options) : base(options?.MiddlewareOptions)
        {
            this.options = options;
            this.stunResponseHandler = stunResponseHandler ?? throw new ArgumentNullException(nameof(stunResponseHandler));
        }

        protected override async Task ProcessAsync(Datagram datagram)
        {
            await NextAsync(datagram);

            var receivedMessage = new STUNMessage(STUNMessageTypes.BindingErrorResponse, Array.Empty<byte>());

            if (!receivedMessage.TryParse(datagram.Buffer.ToArray()))
            {
                return;
            }

            if (receivedMessage.MessageType != STUNMessageTypes.BindingResponse)
            {
                return;
            }

            var receivedTransactionId = new Guid(receivedMessage.TransactionID);

            if (receivedTransactionId != options.TransactionId)
            {
                return;
            }

            var mappedAddressAttribute = receivedMessage.Attributes
                                                        .OfType<STUNMappedAddressAttribute>()
                                                        .FirstOrDefault();

            if (mappedAddressAttribute == null)
            {
                return;
            }

            await stunResponseHandler(new StunResponse
            {
                PublicAddress = mappedAddressAttribute.EndPoint
            });
        }
    }
}
