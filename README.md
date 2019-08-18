# Datagrammer.Stun

If you want to know more about STUN please read this [wiki](https://en.wikipedia.org/wiki/STUN). Datagrammer.Stun helps you to use STUN protocol and public servers for public ip address discovering.

### Getting started

Install from [NuGet](https://www.nuget.org/packages/Datagrammer.Stun/):

```powershell
Install-Package Datagrammer.Stun
```

Use namespace

```csharp
using Datagrammer.Stun;
using Stun.Protocol;
```

### Initialization

Building message:

```csharp
var stunMessage = new StunBuilderStep().SetType(StunMessageType.BindingRequest)
                                       .SetTransactionId(StunTransactionId.Generate())
                                       .Build();
```

Receiving message:

```csharp
var stunMessagePipe = new StunPipeBlock();
var targetBlock = new ActionBlock<StunMessage>(message =>
{
    if(message.Type != StunMessageType.BindingResponse || message.TransactionId != transationId)
    {
        return;
    }
    
    foreach(var attribute in message.Attributes)
    {
        if(StunMappedAddressAttribute.TryParse(attribute, out var mappedAddressAttribute))
        {
            Console.WriteLine(new IPEndPoint(new IPAddress(mappedAddressAttribute.EndPoint.Address.ToArray()), mappedAddressAttribute.EndPoint.Port));
        }
    }
});
stunMessagePipe.LinkTo(targetBlock);
datagramBlock.LinkTo(stunMessagePipe);
```

### License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
