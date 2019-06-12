using Datagrammer.Middleware;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Datagrammer.Stun
{
    public sealed class StunPipeBlock : MiddlewareBlock<Datagram, Datagram>, ISourceBlock<StunResponse>
    {
        private readonly IPropagatorBlock<StunResponse, StunResponse> responseBuffer;

        private readonly StunTransactionId transactionId;

        public StunPipeBlock(StunPipeOptions options) : base(options?.MiddlewareOptions)
        {
            transactionId = options.TransactionId;

            responseBuffer = new BufferBlock<StunResponse>(new DataflowBlockOptions
            {
                BoundedCapacity = options.ResponseBufferCapacity
            });
        }

        protected override async Task ProcessAsync(Datagram datagram)
        {
            await NextAsync(datagram);

            if(!StunMessage.TryParse(datagram.Buffer, out var message))
            {
                return;
            }

            if(message.Type != StunMessageType.BindingResponse)
            {
                return;
            }

            if(message.TransactionId != transactionId)
            {
                return;
            }

            var response = new StunResponse();

            foreach(var attribute in message.Attributes)
            {
                TryParseMappedAddressAttribute(attribute, response);
            }

            await responseBuffer.SendAsync(response);
        }

        private void TryParseMappedAddressAttribute(StunAttribute attribute, StunResponse context)
        {
            if(attribute.Type != StunAttributeType.MappedAddress)
            {
                return;
            }

            if (StunIPEndPointParser.TryParse(attribute.Content.Span, out var endPoint))
            {
                context.PublicAddress = endPoint;
            }
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
