using Datagrammer.Middleware;
using Stun.Protocol;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Datagrammer.Stun
{
    public sealed class StunPipeBlock : MiddlewareBlock<Datagram, Datagram>, ISourceBlock<StunMessage>
    {
        private readonly IPropagatorBlock<StunMessage, StunMessage> responseBuffer;

        public StunPipeBlock() : this(new StunPipeOptions())
        {
        }

        public StunPipeBlock(StunPipeOptions options) : base(options?.MiddlewareOptions)
        {
            responseBuffer = new BufferBlock<StunMessage>(new DataflowBlockOptions
            {
                BoundedCapacity = options.ResponseBufferCapacity
            });
        }

        protected override async Task ProcessAsync(Datagram datagram)
        {
            await NextAsync(datagram);

            if(StunMessage.TryParse(datagram.Buffer, out var message))
            {
                await responseBuffer.SendAsync(message);
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

        public StunMessage ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<StunMessage> target, out bool messageConsumed)
        {
            return responseBuffer.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public IDisposable LinkTo(ITargetBlock<StunMessage> target, DataflowLinkOptions linkOptions)
        {
            return responseBuffer.LinkTo(target, linkOptions);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<StunMessage> target)
        {
            responseBuffer.ReleaseReservation(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<StunMessage> target)
        {
            return ReserveMessage(messageHeader, target);
        }
    }
}
