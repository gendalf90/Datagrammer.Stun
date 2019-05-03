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
            var datagramBlock = new DatagramBlock(new DatagramOptions
            {
                ListeningPoint = new IPEndPoint(IPAddress.Any, 50000)
            });
            var stunMessagesGenerator = new StunGeneratorBlock(new StunGeneratorOptions
            {
                TransactionId = transationId,
                Servers = new[]
                {
                    new IPEndPoint(IPAddress.Parse("217.10.68.145"), 3478), //stun.sipgate.net:3478
                    new IPEndPoint(IPAddress.Parse("64.233.161.127"), 19302), //stun1.l.google.com:19302
                    new IPEndPoint(IPAddress.Parse("77.72.169.213"), 3478) //stun.internetcalls.com:3478
                },
                MessageSendingPeriod = TimeSpan.FromSeconds(1)
            });
            var stunMessagePipe = new StunPipeBlock(new StunPipeOptions
            {
                TransactionId = transationId
            });
            var receivedResponses = new List<StunResponse>();
            var responseReceivingAction = new ActionBlock<StunResponse>(response =>
            {
                receivedResponses.Add(response);
            });
            stunMessagesGenerator.LinkTo(datagramBlock, new DataflowLinkOptions { PropagateCompletion = true });
            datagramBlock.LinkTo(stunMessagePipe, new DataflowLinkOptions { PropagateCompletion = true });
            stunMessagePipe.LinkTo(responseReceivingAction, new DataflowLinkOptions { PropagateCompletion = true });
            stunMessagePipe.LinkTo(DataflowBlock.NullTarget<Datagram>(), new DataflowLinkOptions { PropagateCompletion = true });

            datagramBlock.Start();
            await datagramBlock.Initialization;
            await Task.Delay(4000);
            stunMessagesGenerator.Complete();
            await Task.WhenAll(stunMessagesGenerator.Completion,
                               datagramBlock.Completion,
                               stunMessagePipe.Completion,
                               responseReceivingAction.Completion);

            receivedResponses.Should().HaveCountGreaterOrEqualTo(9);
        }
    }
}
