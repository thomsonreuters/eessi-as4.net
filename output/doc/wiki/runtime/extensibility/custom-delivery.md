# Custom Delivery

When the received payloads must be delivered to a target that is not supported out of the box, a custom deliver implementation needs to be injected. Each deliverer implements the `IDeliverSender` interface:

```csharp
/// <summary>
/// Interface to describe where the <see cref="DeliverMessage"/> has to be send
/// </summary>
public interface IDeliverSender
{
    /// <summary>
    /// Configure the <see cref="IDeliverSender"/>
    /// with a given <paramref name="method"/>
    /// </summary>
    /// <param name="method"></param>
    void Configure(Method method);

    /// <summary>
    /// Start sending the <see cref="DeliverMessage"/>
    /// </summary>
    /// <param name="deliverMessage"></param>
    Task<SendResult> SendAsync(DeliverMessageEnvelope deliverMessage);
}
```

Each deliverer must be configured with a Method configuration. This is the configuration setting in the Receiving Pmode:

```xml
<PMode xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
       xmlns:xsd="http://www.w3.org/2001/XMLSchema"
       xmlns="eu:edelivery:as4:pmode">
    <MessageHandling>
        <Deliver>
            <IsEnabled>true</IsEnabled>
            <DeliverMethod>
                <Type>SERVICEBUS-QUEUE</Type>
                <Parameters>
                    <Parameter name="ConnectionString" value="" />
                    <Parameter name="Queue" value="" />
                </Parameters>
            </DeliverMethod>
        </Deliver>
    </MessageHandling>
</PMode>
```

And for example, implement it as an Azure ServiceBus deliverer:

```csharp
public class ServiceBusSender : IDeliverSender, IDisposable
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private QueueClient _queue;

    public void Configure(Method method)
    {
        _queue = new QueueClient(
            method["ConnectionString"].Value,
            method["Queue"].Value);
    }

    public async Task<SendResult> SendAsync(DeliverMessageEnvelope envelope)
    {
        var msg = new Message(envelope.DeliverMessage)
        {
            MessageId = envelope.MessageInfo.MessageId
        };

        await _queue.SendAsync(msg);

        Logger.Info($"[{envelope.MessageInfo.MessageId}] Send Deliver Message to Service Bus");

        return SendResult.Success;
    }

    public void Dispose()
    {
        _queue.CloseAsync().Wait();
    }
}
```

The `DeliverMessageEnvelope` contains the "to-be-delivered" content. It contains the original message Id and a byte array of the actual delivermessage.

The `IDeliverSender` implementations can return different kinds of `SendResult`'s:

- `SendResult.Success` which means that the _DeliverMessage_ is successfully send.
- `SendResult.RetryableFail` which means that the _DeliverMessage_ can't be uploaded but could succeed with another try. If the _Receiving Processing Mode_ is configured for retryable delivery (see the _Reliability_ child in the _Deliver_ element), the uploading of the _DeliverMessage_ will be retried.
- `SendResult.FatalFail` which means that the _DeliverMessage_ can't be uploaded and will not be retried even if the _Receiving Processing Mode_ is configured this way (see the _Reliability_ child in the _Deliver_ element).

#### Registration

Now, you may have noticed that the configuration section also contains a **Type**. In our example, this was `"SERVICEBUS-QUEUE"`. Each custom deliverer must be configured in the `Registry`.

```xml
<PMode xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
       xmlns:xsd="http://www.w3.org/2001/XMLSchema"
       xmlns="eu:edelivery:as4:pmode">
    <MessageHandling>
        <Deliver>
            <IsEnabled>true</IsEnabled>
            <DeliverMethod>
                <Type>SERVICEBUS-QUEUE</Type>
                <Parameters>
                    <Parameter name="ConnectionString" value="" />
                    <Parameter name="Queue" value="" />
                </Parameters>
            </DeliverMethod>
        </Deliver>
    </MessageHandling>
</PMode>
```

So, our custom deliver implementation could be registered like this:

```csharp
Registry.Instance.DeliverSenderProvider.Accept(
    condition: type => type == "SERVICEBUS-QUEUE",
    sender: () => new ServiceBusSender());
```

This way, the `ServiceBusSender` type can be used by the <b>AS4.NET</b> component.
