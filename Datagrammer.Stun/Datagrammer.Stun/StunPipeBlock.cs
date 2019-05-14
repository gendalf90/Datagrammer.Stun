using Datagrammer.Middleware;
using LumiSoft.Net.STUN.Message;
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
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            if (options.TransactionId == null)
            {
                throw new ArgumentNullException(nameof(options.TransactionId));
            }

            if (options.TransactionId.Length != 12)
            {
                throw new ArgumentException("Transaction id must be with length 12");
            }

            responseBuffer = new BufferBlock<StunResponse>(new DataflowBlockOptions
            {
                BoundedCapacity = options.ResponseBufferCapacity
            });
        }

        protected override async Task ProcessAsync(Datagram datagram)
        {
            await NextAsync(datagram);

            if (!TryParseStunMessage(datagram.Buffer.ToArray(), out var receivedMessage))
            {
                return;
            }

            if (receivedMessage.Type != STUN_MessageType.BindingResponse)
            {
                return;
            }

            if (!receivedMessage.TransactionID.SequenceEqual(options.TransactionId))
            {
                return;
            }

            if (receivedMessage.MappedAddress == null)
            {
                return;
            }

            await responseBuffer.SendAsync(new StunResponse
            {
                PublicAddress = receivedMessage.MappedAddress
            });
        }

        private bool TryParseStunMessage(byte[] bytes, out STUN_Message message)
        {
            message = new STUN_Message();

            try
            {
                message.Parse(bytes);
                return true;
            }
            catch
            {
                return false;
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
