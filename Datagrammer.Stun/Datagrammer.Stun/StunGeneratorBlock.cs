﻿using STUN;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Datagrammer.Stun
{
    public sealed class StunGeneratorBlock : ISourceBlock<Datagram>
    {
        private readonly IPropagatorBlock<Datagram, Datagram> sendingBuffer;
        private readonly Timer sendingTimer;

        public StunGeneratorBlock(StunGeneratorOptions options)
        {
            if(options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if(options.Server == null)
            {
                throw new ArgumentException(nameof(options.Server), "Server must be set");
            }

            sendingBuffer = new BufferBlock<Datagram>(new DataflowBlockOptions
            {
                BoundedCapacity = 1
            });

            var stunMessage = new STUNMessage(STUNMessageTypes.BindingRequest, options.TransactionId.ToByteArray());
            var stunDatagram = new Datagram(stunMessage.GetBytes(), options.Server);

            sendingTimer = new Timer(PostMessage, stunDatagram, options.MessageSendingPeriod, options.MessageSendingPeriod);

            Completion = CompleteAsync();
        }

        private void PostMessage(object state)
        {
            sendingBuffer.Post((Datagram)state);
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
