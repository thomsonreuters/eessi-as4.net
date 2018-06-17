# Custom Payload Retrieval

Inside a **Submit Message**, a reference to a Payload can be included. This payload can be located on the file system, can be on a remote server, … In order that the <b>AS4.NET</b> component knows where to look for, it has registered some **Payload Retrievers**.

```xml
<SubmitMessage>
    <Payloads>
        <Id>photo</Id>
        <MimeType>image/jpeg</MimeType>
        <Location>>https://accountname.blob.core.windows.net/photo.jpg</Location>
    </Payloads>
</SubmitMessage>
```

Each retriever is responsible for retrieving a payload from a certain source. Custom retrievers can be made for a source
Here’s the interface that must be implemented:

```csharp
/// <summary>
/// Interface that defines how a payload must be retrieved from a certain location.
/// </summary>
public interface IPayloadRetriever
{
    /// <summary>
    /// Retrieve <see cref="Stream"/> contents from a given <paramref name="location"/>.
    /// </summary>
    /// <param name="location">The location.</param>
    /// <returns></returns>
    Task<Stream> RetrievePayloadAsync(string location);
}
```

To implement a retriever that gets payloads from **Azure Blob Storage**, we could write something like this:

```csharp
public class BlobPayloadRetriever : IPayloadRetriever
{
    private readonly CloudBlobContainer _container;

    public BlobPayloadRetriever()
    {
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            payloadReferenceMethod["ConnectionString"].Value);

        CloudBlobClient client = storageAccount.CreateCloudBlobClient();
        _container = client.GetContainerReference("Payloads");
        _container.CreateIfNotExists();
    }

    public async Task<Stream> RetrievePayloadAsync(string location)
    {
        CloudBlob blob = _container.GetBlobReference(location);

        var str = new VirtualStream(VirtualStream.MemoryFlag.AutoOverflowToDisk);
        await blob.DownloadToStreamAsync(str);

        return str;
    }
}
```

#### Registration

Some retrievers are already registered: <b>AS4.NET</b> has `IPayloadRetriever` instances registered that can retrieve files from the filesystem and from a web location. Custom retrievers must be registered inside the `Registry` instance.

```csharp
Registry.Instance.PayloadRetrieverProvider.Accept(
    condition: p => p.Location.Contains("blob"),
    retriever: new BlobPayloadRetriever());
```

The location were the payload can be found, is available in the **Submit/Deliver Message**:

```xml
<SubmitMessage>
    <Payloads>
        <Id>my-photo</Id>
        <MimeType>image/jpeg</MimeType>
        <Location>>blob::///https://accountname.blob.core.windows.net/myphoto.jpg</Location>
    </Payloads>
</SubmitMessage>
```
