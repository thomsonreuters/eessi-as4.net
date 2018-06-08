# Settings

The `settings.xml` located inside the `.\config\` folder contains several global configuration settings used inside the component.
Below you find a pseudo example of the settings shipped with the component itself. It contains onlyl a `<SubmitAgent />` but  other types of agents are also available (See the section below about **Agents**).

```xml
<?xml version="1.0" encoding="utf-8"?>
<Settings>
    <IdFormat>{GUID}@{IPADDRESS}</IdFormat>
    <FeInProcess>true</FeInProcess>
    <PayloadServiceInProcess>true</PayloadServiceInProcess>
    <RetentionPeriod>90</RetentionPeriod>
    <Database>
        <Provider>Sqlite</Provider>
        <ConnectionString>Filename=database\messages.db</ConnectionString>
    </Database>
    <CertificateStore>
        <StoreName>My</StoreName>
        <Repository type="Eu.EDelivery.AS4.Repositories.CertificateRepository" />
    </CertificateStore>
    <Agents>
        <SubmitAgent name="FILE Submit Agent">
            <Receiver type="Eu.EDelivery.AS4.Receivers.FileReceiver">
                <Setting key="FilePath">.\messages\out</Setting>
                <Setting key="FileMask">*.xml</Setting>
                <Setting key="PollingInterval">0:00:05</Setting>
            </Receiver>
            <Transformer type="Eu.EDelivery.AS4.Transformers.SubmitMessageXmlTransformer" />
            <StepConfiguration>
                <NormalPipeline>
                    <Step type="Eu.EDelivery.AS4.Steps.Submit.RetrieveSendingPModeStep" />
                    <Step type="Eu.EDelivery.AS4.Steps.Submit.DynamicDiscoveryStep"/>
                    <Step type="Eu.EDelivery.AS4.Steps.Submit.CreateAS4MessageStep" />
                    <Step type="Eu.EDelivery.AS4.Steps.Submit.StoreAS4MessageStep" />
                </NormalPipeline>
            </StepConfiguration>
        </SubmitAgent>
    </Agents>
```

The different kind of settings are explained in the following paragraphs:


### GUID Format

When creating AS4 Messages, Message Ids are being generated. To configure the format in which this must be done the `<IdFormat/>` tag is being used inside the `settings.xml`.

Default: `{GUID}@{IPADDRESS}`

### Payload Service In Process

This setting defines whether or not the (optional) PayloadService should be started in-process with the <b>AS4.NET</b> MessageHandler.

The PayloadService is a REST service that can contain payloads that are referenced by submit-messages.

### FE In Process

This setting defines whether or not the <b>AS4.NET</b> FrontEnd should be started in-process with the <b>AS4.NET</b> MessageHandler.
The <b>AS4.NET</b> FrontEnd web-application lets you configure the <b>AS4.NET</b> messagehandler and offers monitoring functionality.

### Retention Period (in days)

The settings contains a retention period number (in days) that can be used to manipulate when stored records must be deleted (hard delete) from the datastore.

So specifying `<RetentionPeriod>10</RetentionPeriod>` will make sure that records older than **10 days** will be deleted from the datastore.

The default value for this period is: **90 days**.

> The `<RetentionPeriod/>` tag is available from version v3.0.0 and up.

### Retry Reliability

The retry reliability specifies also some settings that manipulates the retry mechanism of the <b>AS4.NET</b> component. Retries happen when messages/exceptions gets *Notified* or *Delivered*.

Within the `<RetryReliability/>` tag you can specify the following settings:

* **Polling Interval**: defines the interval in which the retry mechanism of the <b>AS4.NET</b> component should be triggered.

Example of a **Retry Reliability** configuration with a **Polling Interval** of 5 seconds:
```xml
<RetryReliability>
    <PollingInterval>00:00:05</PollingInterval>
</RetryReliability>
```

> The `<RetryReliability/>` tag is available form version vXXX and up.

### Database Provider

The component can be configured to store messages and exceptions in another datastore. Inside the `settings.xml` the `<Database/>` tag is responsible for this. Underneath this tag you define the `<Provider/>`, which can be **SQLite**, **SQLServer**,… any type which is supported in **Entity Framework Core**; and the <ConnectionString/> which defines the actual connection to the database.

So, let’s say you want to change the provider to store the messages in a SQL Server database; you must change the <Provider/> to “SqlServer” and the <ConnectionString/> to a valid SQL Server Connection String: `Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;`.

- Default Provider: **Sqlite**
- Default Connection String: `Filename=database\messages.db`

Example of a **Database Provider** for **Sqlite**:
```xml
<Database>
    <Provider>Sqlite</Provider>
    <ConnectionString>Filename=database\messages.db</ConnectionString>
</Database>
```

### Certificate Store

To support signing and encrypting messages, certificates are needed. The certificate store that’s needed to retrieve this certificates and be set in the <StoreName/> tag.

Default: **My**

The default implementation is used to retrieve certificates from a certificate store on a Windows environment; but you can write your own implementation. If you’re on a Windows environment (so the default implementation is OK for you) you can define here in which Store you want to search. 

Following values can be used:

| Member name          | Description                                                                 |
| -------------------- | --------------------------------------------------------------------------- |
| AddressBook          | The X.509 certificate store for other users.                                |
| AuthRoot             | The X.509 certificate store for third-party certificate authorities (CAs).  |
| CertificateAuthority | The X.509 certificate store for intermediate certificate authorities (CAs). |
| Disallowed           | The X.509 certificate store for revoked certificates.                       |
| My                   | The X.509 certificate store for personal certificates.                      |
| Root                 | The X.509 certificate store for trusted root certificate authorities (CAs). |
| TrustedPeople        | The X.509 certificate store for directly trusted people and resources.      |
| TrustedPublisher     | The X.509 certificate store for directly trusted publishers.                |

Example of a **Certificate Store** referencing the **My** certificate repository:

```xml
<CertificateStore>
    <StoreName>My</StoreName>
    <Repository type="Eu.EDelivery.AS4.Repositories.CertificateRepository" />
</CertificateStore>
```

### Agents

The AS4 protocol has several operations: Submit, Send, Receive, Deliver and Notify. All of these operations are configured in the settings.xml as Agents. Each agent has three items which defines the agent: **Receiver**, **Transformer** and **Step(s)**.

A **Receiver** can be configured in the `<Receiver/>` tag in each agent. There are multiple kinds of receivers: FileReceiver, DatastoreReceiver, HttpReceiver… Each needed to be configured in order to work correctly. This can be done in the children of this tag as a `<Setting/>` (with attribute **key**; the inner text of the tag is the **value**).

A **Transformer** can be configured in the `<Transformer/>` tag in each agent. This transformer is needed in order to transform the received message (could be `.xml`, `.json`…) and transform it to a AS4 Message that can be used in the Step(s).

A **Step** or **Steps** can be configured in the `<StepConfiguration/>` tag in each agent.  The StepConfiguration element must at least contain a NormalPipeline element.  The **NormalPipeline** element contains the steps that must be executed by the Agent. These steps will be executed after the message is being transformed. Example of steps are: `CreateReceiptStep`, `CompressAttachmentsStep`, `DecryptAS4MessageStep`…

The **StepConfiguration** can contain an **ErrorPipeline** element as well.  This element contains the Steps that will be executed when a step in the **NormalPipeline** failed to execute successfully.

Each tag (**Receiver**, **Transformer** and **Step**) has a **type** attribute which defines the type of which the instance must be created inside the component.

Example of a **Receive Agent** that receives `AS4Message`'s on a configured HTTP endpoint, validates the incoming message, and saves it to the datastore:

> Note that the assembly name of the different classes are truncated for better readability. For the full name, see the actual `settings.xml` file stored in the '`.\config\` folder.

```xml
<ReceiveAgent name="Receive Agent">
    <Receiver type="Eu.EDelivery.AS4.Receivers.HttpReceiver">
        <Setting key="Url">http://localhost:8081/msh/receive/</Setting>
    </Receiver>
    <Transformer type="Eu.EDelivery.AS4.Transformers.ReceiveMessageTransformer" />
    <StepConfiguration>
        <NormalPipeline>
            <Step type="Eu.EDelivery.AS4.Steps.Receive.SaveReceivedMessageStep" />
            <Step type="Eu.EDelivery.AS4.Steps.Receive.DeterminePModesStep" />
            <Step type="Eu.EDelivery.AS4.Steps.Receive.ValidateAS4MessageStep" />
            <Step type="Eu.EDelivery.AS4.Steps.Receive.DecryptAS4MessageStep" />
            <Step type="Eu.EDelivery.AS4.Steps.Receive.VerifySignatureAS4MessageStep" />
            <Step type="Eu.EDelivery.AS4.Steps.Receive.DecompressAttachmentsStep" />
            <Step type="Eu.EDelivery.AS4.Steps.Receive.UpdateReceivedAS4MessageBodyStep" />
            <Step type="Eu.EDelivery.AS4.Steps.Receive.CreateAS4ReceiptStep" />
            <Step type="Eu.EDelivery.AS4.Steps.Send.SignAS4MessageStep" />
            <Step type="Eu.EDelivery.AS4.Steps.Receive.SendAS4SignalMessageStep" />
                </NormalPipeline>
                <ErrorPipeline>
                    <Step type="Eu.EDelivery.AS4.Steps.Receive.CreateAS4ErrorStep" />
                    <Step type="Eu.EDelivery.AS4.Steps.Send.SignAS4MessageStepl" />
                    <Step type="Eu.EDelivery.AS4.Steps.Receive.SendAS4SignalMessageStep" />
                </ErrorPipeline>
            </StepConfiguration>
        </ReceiveAgent>
```

### Custom Settings

When creating custom implementations of types, settings can sometimes be useful. For example, an **Email Sender** by which you configure the **SMTP Server** in the `settings.xml`. This can be useful instead of hardcoded each configured value in the implementation itself.

Example of a collection of custom settings:

```xml
<CustomSettings>
    <Setting key="APIKey">my-api-key</Setting>
    <Setting key="RefreshAPIKey">my-refresh-api-key</Setting>
</CustomSettings>
```
