# Retry Reliability

During the _Delivery_ and _Notification_ of _User_- and _SignalMesssage_'s it could happen that the message is not correctly delivered or notified by the first attempt. Delivery or notification can fail for several reasons, for instance a network timeout.

To provide a more reliable deliver and notification system, AS4.NET supports retrying when delivery or notification fails. The retry deliver and notification functionality of the <b>AS4.NET</b> component is configured in the _Sending Processing Mode_ and _Receiving Processing Mode_.

## Processing Mode Configuration

A `<Reliability/>` element must have the following values configured in this structure:

```xml
<Reliability>
  <IsEnabled>true</IsEnabled>
  <RetryCount>3</RetryCount>
  <RetryInterval>00:00:05</RetryInterval>
</Reliability>
```

- The `IsEnabled` flag manipulates whether or not the operation should be retried on failure
- The `RetryCount` will be the **MaxRetryCount** value used to determine how many retries could happen
- The `RetryInterval` will be the interval in which retries happen

For the _Sending Processing Mode_:

- The `<ReceiptHandling/>` tag can have a `Reliability` element
- The `<ErrorHandling/>` tag can have a `Reliability` element
- The `<ExceptionHandling/>` tag can have a `Reliability` element

For the _Receiving Processing Mode_:

- The `<Deliver/>` tag can have a `Reliability` element
- The `<ExceptionHandling/>` tag can have a `<Reliability/>` element

That's all you need to do to enable the retry-functionality during the delivery or notification process.

> Note that on certain failures, the retry will **NOT** happen even when it's configured this way. This could happen when the failure that occured is considered _FATAL_ (for example: a 401 HTTP response code which means that there's a problem with permissions). The reason for this is that even when an retry is configured, the message will still fail the second time arround, therefore no retry will happen.

> Retryable deliver/notification is only available in version **AS4.NET v.XXX** and beyond.
