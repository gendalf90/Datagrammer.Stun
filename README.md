# Datagrammer.Stun

If you want to know more about STUN please read this [wiki](https://en.wikipedia.org/wiki/STUN). Datagrammer.Stun helps you to use STUN protocol and public servers for public ip address discovering. The [package](https://www.nuget.org/packages/stun/) is used for STUN protocol integration.

### Getting started

Install from [NuGet](https://www.nuget.org/packages/Datagrammer.Stun/):

```powershell
Install-Package Datagrammer.Stun
```

Use namespace

```csharp
using Datagrammer.Stun;
```

### Initialization

Every STUN message has transaction identificator value:

```csharp
var transactionId = Guid.NewGuid();
```

To initialize STUN messages generator use this code:

```csharp
var generator = new StunGeneratorBlock(new StunGeneratorOptions
{
    TransactionId = transactionId,
    Servers = new [] { new IPEndPoint(IPAddress.Parse("64.233.161.127"), 19302) /*stun1.l.google.com:19302*/ },
    MessageSendingPeriod = TimeSpan.FromSeconds(1)
});
```

Also you need STUN message handler. Define it like this:

```csharp
var stunMessageHandler = new StunPipeBlock(new StunPipeOptions
{
    TransactionId = transactionId //needed to identify responses from server
});
```

Note: this block has both interfaces implementation: `ISourceBlock<Datagram>` and `ISourceBlock<StunResponse>`. To avoid buffers blocking consume messages from both of these sources.

### Using

With [Datagrammer](https://github.com/gendalf90/Datagrammer) using example:

```csharp
var datagramBlock = new DatagramBlock(new DatagramOptions
{
    ListeningPoint = new IPEndPoint(IPAddress.Any, 50000)
});
var responseReceivingAction = new ActionBlock<StunResponse>(response =>
{
    Console.WriteLine(response.PublicAddress);
});

generator.LinkTo(datagramBlock);
datagramBlock.LinkTo(stunMessageHandler);
stunMessageHandler.LinkTo(responseReceivingAction);
stunMessageHandler.LinkTo(DataflowBlock.NullTarget<Datagram>()); //because it works like pipe and you need to consume datagrams too

datagramBlock.Start();
```

### License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
