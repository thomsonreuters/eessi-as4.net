# Settings

The `settings.xml` located inside the config folder contains several global configuration settings used inside the component. Each kind of setting is explained in the following paragraphs.

### GUID Format

When creating AS4 Messages, Message Ids are being generated. To configure the format in which this must be done the `<IdFormat/>` tag is being used inside the `settings.xml`.

Default: `{GUID}@{IPADDRESS}`

### Payload Service In Process

This setting defines whether or not the (optional) PayloadService should be started in-process with the <b>AS4.NET</b> MessageHandler.

The PayloadService is a REST service that can contain payloads that are referenced by submit-messages.

### FE In Process

This setting defines whether or not the <b>AS4.NET</b> FrontEnd should be started in-process with the <b><b>AS4.NET</b></b> MessageHandler.
The <b>AS4.NET</b> FrontEnd web-application lets you configure the <b>AS4.NET</b> messagehandler and offers monitoring functionality.

### Database Provider

The component can be configured to store messages and exceptions in another datastore. Inside the `settings.xml` the `<Database/>` tag is responsible for this. Underneath this tag you define the `<Provider/>`, which can be **SQLite**, **SQLServer**,… any type which is supported in **Entity Framework Core**; and the <ConnectionString/> which defines the actual connection to the database.

So, let’s say you want to change the provider to store the messages in a SQL Server database; you must change the <Provider/> to “SqlServer” and the <ConnectionString/> to a valid SQL Server Connection String: `Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;`.

- Default Provider: **Sqlite**
- Default Connection String: `Filename=database\messages.db`

### Certificate Store

To support signing and encrypting messages, certificates are needed. The certificate store that’s needed to retrieve this certificates and be set in the <StoreName/> tag.

Default: **My**

The default implementation is used to retrieve certificates from a certificate store on a Windows environment; but you can write your own implementation. If you’re on a Windows environment (so the default implementation is OK for you) you can define here in which Store you want to search. 

Following values can be used:

| Member name           | Description                                                                 |
| --------------------- | --------------------------------------------------------------------------- |
| AddressBook           | The X.509 certificate store for other users.                                |
| AuthRoot              | The X.509 certificate store for third-party certificate authorities (CAs).  |
| CertificateAuthority  | The X.509 certificate store for intermediate certificate authorities (CAs). |
| Disallowed            | The X.509 certificate store for revoked certificates.                       |
| My                    | The X.509 certificate store for personal certificates.                      |
| Root                  | The X.509 certificate store for trusted root certificate authorities (CAs). |
| TrustedPeople         | The X.509 certificate store for directly trusted people and resources.      |
| TrustedPublisher      | The X.509 certificate store for directly trusted publishers.                |

### Agents

The AS4 protocol has several operations: Submit, Send, Receive, Deliver and Notify. All of these operations are configured in the settings.xml as Agents. Each agent has three items which defines the agent: **Receiver**, **Transformer** and **Step(s)**.

A **Receiver** can be configured in the `<Receiver/>` tag in each agent. There are multiple kinds of receivers: FileReceiver, DatastoreReceiver, HttpReceiver… Each needed to be configured in order to work correctly. This can be done in the children of this tag as a `<Setting/>` (with attribute **key**; the inner text of the tag is the **value**).

A **Transformer** can be configured in the `<Transformer/>` tag in each agent. This transformer is needed in order to transform the received message (could be `.xml`, `.json`…) and transform it to a AS4 Message that can be used in the Step(s).

A **Step** or **Steps** can be configured in the `<StepConfiguration/>` tag in each agent.  The StepConfiguration element must at least contain a NormalPipeline element.  The **NormalPipeline** element contains the steps that must be executed by the Agent. These steps will be executed after the message is being transformed. Example of steps are: `CreateReceiptStep`, `CompressAttachmentsStep`, `DecryptAS4MessageStep`…

The **StepConfiguration** can contain an **ErrorPipeline** element as well.  This element contains the Steps that will be executed when a step in the **NormalPipeline** failed to execute successfully.

Each tag (**Receiver**, **Transformer** and **Step**) has a **type** attribute which defines the type of which the instance must be created inside the component.

### Custom Settings

When creating custom implementations of types, settings can sometimes be useful. For example, an **Email Sender** by which you configure the **SMTP Server** in the `settings.xml`. This can be useful instead of hardcoded each configured value in the implementation itself.
