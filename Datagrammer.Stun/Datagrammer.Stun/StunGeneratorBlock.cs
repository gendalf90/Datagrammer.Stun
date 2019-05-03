using STUN;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Datagrammer.Stun
{
    public sealed class StunGeneratorBlock : ISourceBlock<Datagram>
    {
        private readonly StunGeneratorOptions options;
        private readonly IPropagatorBlock<Datagram, Datagram> sendingBuffer;
        private readonly Timer sendingTimer;
        private readonly byte[] stunRequestBytes;

        public StunGeneratorBlock(StunGeneratorOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            sendingBuffer = new BufferBlock<Datagram>(new DataflowBlockOptions
            {
                BoundedCapacity = options.Servers.Length
            });

            sendingTimer = new Timer(SendMessageSafeAsync, null, options.MessageSendingPeriod, Timeout.InfiniteTimeSpan);

            stunRequestBytes = new STUNMessage(STUNMessageTypes.BindingRequest, options.TransactionId.ToByteArray()).GetBytes();

            Completion = CompleteAsync();
        }

        private async void SendMessageSafeAsync(object state)
        {
            try
            {
                await SendMessagesAsync();
                RestartTimer();
            }
            catch(Exception e)
            {
                Fault(e);
            }
        }

        private async Task SendMessagesAsync()
        {
            var sendingTasks = options.Servers
                                      .Select(server => new Datagram
                                      {
                                          Bytes = stunRequestBytes,
                                          EndPoint = server
                                      })
                                      .Select(sendingBuffer.SendAsync);

            await Task.WhenAll(sendingTasks);
        }

        private void RestartTimer()
        {
            sendingTimer.Change(options.MessageSendingPeriod, Timeout.InfiniteTimeSpan);
        }

        public Task Completion { get; private set; }

        public void Complete()
        {
            sendingBuffer.Complete();
        }

        private async Task CompleteAsync()
        {
            using (sendingTimer)
            {
                await sendingBuffer.Completion;
            }
        }

        public Datagram ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<Datagram> target, out bool messageConsumed)
        {
            return sendingBuffer.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public void Fault(Exception exception)
        {
            sendingBuffer.Fault(exception);
        }

        public IDisposable LinkTo(ITargetBlock<Datagram> target, DataflowLinkOptions linkOptions)
        {
            return sendingBuffer.LinkTo(target, linkOptions);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<Datagram> target)
        {
            sendingBuffer.ReleaseReservation(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<Datagram> target)
        {
            return sendingBuffer.ReserveMessage(messageHeader, target);
        }
    }
}
