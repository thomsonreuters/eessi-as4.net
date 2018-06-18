# Uploading Payloads to a Custom Location

When a message must be delivered, it can contain payloads. The <b>AS4.NET</b> component can deliver payloads to some predefined locations (filesystem, payloadservice) and can be extended to upload payloads to custom locations.
Each uploader must implement following interface:

```csharp
/// <summary>
/// Interface to upload Payloads to a given Media
/// </summary>
public interface IAttachmentUploader
{
   /// <summary>
   /// Configure the <see cref="IAttachmentUploader"/>
   /// with a given <paramref name="payloadReferenceMethod"/>
   /// </summary>
   /// <param name="payloadReferenceMethod"></param>
   void Configure(Method payloadReferenceMethod);

   /// <summary>
   /// Start uploading the <paramref name="attachment"/>
   /// </summary>
   /// <remarks>The <paramref name="referringUserMessage"/> parameter can be used
   /// by the IAttachmentUploader implementation when determining the name that must be
   /// given to the uploaded payload.</remarks>
   /// <param name="attachment">The <see cref="Attachment"/> that must be uploaded</param>
   /// <param name="referringUserMessage">The UserMessage to which the Attachment belongs to.</param>
   /// <returns>An UploadResult instance</returns>
   Task<UploadResult> UploadAsync(Attachment attachment, UserMessage referringUserMessage);
}
```

Since the **Deliver Message** references the location of the uploaded payloads, the uploader returns an **Upload Result**. This contains a reference and an id that can be included in the **Deliver Message**.

Each uploader can be configured with settings that can be found in the **Receiving PMode**:

```xml
<PMode xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
       xmlns:xsd="http://www.w3.org/2001/XMLSchema"
       xmlns="eu:edelivery:as4:pmode">
    <MessageHandling>
        <Deliver>
            <IsEnabled>true</IsEnabled>
            <PayloadReferenceMethod>
                <Type>BLOB</Type>
                <Parameters>
                    <Parameter name="ConnectionString" value="" />
                    <Parameter name="Container" value="" />
                </Parameters>
            </PayloadReferenceMethod>
        </Deliver>
    </MessageHandling>
</PMode>
```

If payloads have to be uploaded to **Azure Blob Storage**, an `IAttachmentUploader` can be implemented that looks like this:

```csharp
public class BlobAttachmentUploader : IAttachmentUploader
{
    private CloudBlobContainer _container;

    public void Configure(Method payloadReferenceMethod)
    {
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            payloadReferenceMethod["ConnectionString"].Value);

        CloudBlobClient client = storageAccount.CreateCloudBlobClient();
        _container = client.GetContainerReference(payloadReferenceMethod["Container"].Value);
        _container.CreateIfNotExists();
    }

    public async Task<UploadResult> UploadAsync(
        Attachment attachment,
        UserMessage referringUserMessage)
    {
        CloudBlockBlob blob = _container.GetBlockBlobReference(attachment.Id);
        await blob.UploadFromStreamAsync(attachment.Content);

        return UploadResult.SuccessWithIdAndUrl(
            payloadId: blob.Name,
            downloadUrl: blob.Uri.AbsolutePath
        );
    }
}
```

The `IAttachmentUploader` can return different types of `UploadResult`'s:

- `UploadResult.SuccessWithUrl(downloadUrl : string)` which means that the attachment is successfully uploaded and we get an download url where the attachment can be retrieved.
- `UploadResult.RetryableFail` which means that the attachment can't be uploaded but could succeed with another try. If the _Receiving Processing Mode_ is configured for retryable delivery, the uploading of the attachment will be retried.
- `UploadFatalFail` which means that the attachment can't be uploaded and will not be retried even if the _Receiving Processing Mode_ is configured this way.

#### Registration

To make sure that the <b>AS4.NET</b> component understands how to upload attachments to **Blob Storage**, we must register the custom `IAttachmentUploader` type in the `Registry`.

```csharp
Registry.Instance.AttachmentUploader.Accept(
    condition: s => s == "BLOB",
    uploader: new BlobAttachmentUploader());
```

This `"BLOB"` string will correspond with the Type configured in the **Receiving PMode**.

```xml
<PMode xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
       xmlns:xsd="http://www.w3.org/2001/XMLSchema"
       xmlns="eu:edelivery:as4:pmode">
    <MessageHandling>
        <Deliver>
            <IsEnabled>true</IsEnabled>
            <PayloadReferenceMethod>
                <Type>BLOB</Type>
                <Parameters>
                    <Parameter name="ConnectionString" value="" />
                    <Parameter name="Container" value="" />
                </Parameters>
            </PayloadReferenceMethod>
        </Deliver>
    </MessageHandling>
</PMode>
```
