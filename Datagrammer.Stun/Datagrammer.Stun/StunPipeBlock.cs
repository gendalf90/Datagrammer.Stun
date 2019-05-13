using Datagrammer.Middleware;
using STUN;
using STUN.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Datagrammer.Stun
{
    public sealed class StunPipeBlock : MiddlewareBlock<Datagram, Datagram>, ISourceBlock<StunResponse>
    {
        private readonly StunPipeOptions options;
        private readonly IPropagatorBlock<StunResponse, StunResponse> responseBuffer;

        public StunPipeBlock(StunPipeOptions options) : base(options?.MiddlewareOptions)
        {
            this.options = options;

            responseBuffer = new BufferBlock<StunResponse>(new DataflowBlockOptions
            {
                BoundedCapacity = options.ResponseBufferCapacity
            });
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

            await responseBuffer.SendAsync(new StunResponse
            {
                PublicAddress = mappedAddressAttribute.EndPoint
            });
        }

        protected override Task AwaitCompletionAsync()
        {
            return responseBuffer.Completion;
        }

        protected override void OnComplete()
        {
            responseBuffer.Complete();
        }

        protected override void OnFault(Exception exception)
        {
            responseBuffer.Fault(exception);
        }

        public StunResponse ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<StunResponse> target, out bool messageConsumed)
        {
            return responseBuffer.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public IDisposable LinkTo(ITargetBlock<StunResponse> target, DataflowLinkOptions linkOptions)
        {
            return responseBuffer.LinkTo(target, linkOptions);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<StunResponse> target)
        {
            responseBuffer.ReleaseReservation(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<StunResponse> target)
        {
            return ReserveMessage(messageHeader, target);
        }
    }
}
