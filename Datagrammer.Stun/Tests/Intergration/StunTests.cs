using Datagrammer;
using Datagrammer.Stun;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace Tests.Integration
{
    public class StunTests
    {
        [Fact]
        public async Task SendingAndReceiving()
        {
            var transationId = Guid.NewGuid();
            var receivedResponses = new List<StunResponse>();
            var datagramBlock = new DatagramBlock(new DatagramOptions
            {
                ListeningPoint = new IPEndPoint(IPAddress.Any, 50000)
            });
            var stunMessagesGenerator = new StunGeneratorBlock(new StunGeneratorOptions
            {
                TransactionId = transationId,
                Server = new IPEndPoint(IPAddress.Parse("64.233.161.127"), 19302), //stun1.l.google.com:19302
                MessageSendingPeriod = TimeSpan.FromSeconds(1)
            });
            var stunMessagePipe = new StunPipeBlock(response =>
            {
                receivedResponses.Add(response);
                return Task.CompletedTask;
            }, new StunPipeOptions { TransactionId = transationId });
            stunMessagesGenerator.LinkTo(datagramBlock, new DataflowLinkOptions { PropagateCompletion = true });
            datagramBlock.LinkTo(stunMessagePipe, new DataflowLinkOptions { PropagateCompletion = true });
            stunMessagePipe.LinkTo(DataflowBlock.NullTarget<Datagram>());

            datagramBlock.Start();
            await datagramBlock.Initialization;
            await Task.Delay(4000);
            stunMessagesGenerator.Complete();
            await Task.WhenAll(stunMessagesGenerator.Completion,
                               datagramBlock.Completion,
                               stunMessagePipe.Completion);

            receivedResponses.Should().HaveCountGreaterOrEqualTo(3);
        }
    }
}
