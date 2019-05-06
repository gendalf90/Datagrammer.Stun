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

Each STUN message has transaction identificator value:

```csharp
var transactionId = Guid.NewGuid();
```

To initialize STUN messages generator use this code:

```csharp
var generator = new StunGeneratorBlock(new StunGeneratorOptions
{
    TransactionId = transactionId,
    Server = new IPEndPoint(IPAddress.Parse("64.233.161.127"), 19302), //stun1.l.google.com:19302
    MessageSendingPeriod = TimeSpan.FromSeconds(1) //message will be created after each of these periods
});
```

Also you need STUN message handler. Define it like this:

```csharp
Func<StunResponse, Task> responseHandlingFunc = response =>
{
    Console.WriteLine(response.PublicAddress);
    return Task.CompletedTask;
};

var stunMessageHandler = new StunPipeBlock(responseHandlingFunc, new StunPipeOptions
{
    TransactionId = transactionId //needed to identify responses from server
});
```

### Using

With [Datagrammer](https://github.com/gendalf90/Datagrammer) using example:

```csharp
var datagramBlock = new DatagramBlock(new DatagramOptions
{
    ListeningPoint = new IPEndPoint(IPAddress.Any, 50000)
});

generator.LinkTo(datagramBlock);
datagramBlock.LinkTo(stunMessageHandler);
stunMessageHandler.LinkTo(DataflowBlock.NullTarget<Datagram>()); //because it works like pipe and you need to consume datagrams that buffer isn't blocked

datagramBlock.Start();
```

### License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
