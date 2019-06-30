using Datagrammer;
using Datagrammer.Stun;
using FluentAssertions;
using Stun.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
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
            var transationId = StunTransactionId.Generate();
            var mappedAddresses = new List<IPEndPoint>();
            var datagramBlock = new DatagramBlock(new DatagramOptions
            {
                ListeningPoint = new IPEndPoint(IPAddress.Any, 50000)
            });
            var stunMessage = new StunBuilderStep().SetType(StunMessageType.BindingRequest)
                                                   .SetTransactionId(transationId)
                                                   .Build();
            var datagram = new Datagram(stunMessage, new byte[] { 64, 233, 161, 127 }, 19302); //stun1.l.google.com:19302
            var stunMessageHandler = new ActionBlock<StunMessage>(response =>
            {
                if(response.Type != StunMessageType.BindingResponse || response.TransactionId != transationId)
                {
                    return;
                }

                foreach(var attribute in response.Attributes)
                {
                    if(StunMappedAddressAttribute.TryParse(attribute, out var mappedAddressAttribute))
                    {
                        mappedAddresses.Add(new IPEndPoint(new IPAddress(mappedAddressAttribute.EndPoint.Address.ToArray()), mappedAddressAttribute.EndPoint.Port));
                    }
                }
            });
            var stunMessagePipe = new StunPipeBlock();
            datagramBlock.LinkTo(stunMessagePipe, new DataflowLinkOptions { PropagateCompletion = true });
            stunMessagePipe.LinkTo(stunMessageHandler, new DataflowLinkOptions { PropagateCompletion = true });
            stunMessagePipe.LinkTo(DataflowBlock.NullTarget<Datagram>());

            using(new Timer(context => datagramBlock.Post(datagram), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)))
            {
                datagramBlock.Start();
                await datagramBlock.Initialization;
                await Task.Delay(4000);
                datagramBlock.Complete();
                await Task.WhenAll(datagramBlock.Completion,
                                   stunMessagePipe.Completion,
                                   stunMessageHandler.Completion);
            }

            mappedAddresses.Should().HaveCountGreaterOrEqualTo(3);
        }
    }
}
