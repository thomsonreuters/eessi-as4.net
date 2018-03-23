# Configuration

The paragraphs in this section describe the contents of the package and provide a brief overview of the configuration.

## Package

The package itself is divided in several folders:

* **config**
* **database**
* **documentation**
* **logs**
* **messages**
* **samples**
* **component.exe**

In the root of the package you find the .exe file that runs the component. Next up all the folders are being explained.

### Config Folder

Inside the configuration folder, following structure is created:

* **receive-pmodes**
* **send-pmodes**
* **settings.xml**

The folders send/receive-pmodes are the folders which configured the PModes (respectively send/receive). Samples of these PModes can be found in the Samples folder.
The **settings.xml** file contains the global configuration of the component and is explained below (2.2).

### Database Folder

Default **SQLite** is used as database. The .db file which contains the SQLite database is stored in this folder.
Regardless of the database that is being used, this folder will by default also contain the following folder structure:

* **as4messages**
    * **in**
    * **out**

Inside these folders, the messagebodies of the AS4 messages that have been sent and received are saved.
Received messages are saved in the **in** folder, messages that have been sent are saved in the **out** folder.

### Documentation Folder

Inside the documentation folder, following structure is created:

* schemas

Inside the **schemas** folder the .xsd files are located of the **PModes** and **messages**. In the root of this folder, documentation is added.

### Logs

Inside the logs folder, you can find detailed debug and error logs.  In order to modify the log configuration, follow the instructions in the section: *"Configure Logging"*.

### Messages Folder

Inside the messages folder, following structure is created:

* **attachments**
* **errors**
* **exceptions**
* **receipts**
* **in**
* **out**

The **attachments** folder contains several files (pictures and .xml documents) that are being used as reference for the send AS4 messages. The **receipts/errors/exceptions** folders are used to store **Notify Messages**. The in folder is used to store incoming messages and attachments; the **out** folder is being used to send messages to another MSH (the .xml file will be renamed to `.accepted` if it’s being retrieved by the component).

These folders are used just to get started with the component.  The component can be configured to use other file folders.

### Samples Folder

Inside the samples folder, following structure is created:

* **certificates**
* **messages**
* **receive-pmodes**
* **send-pmodes**

Each folder contains the respectively the samples of send/receive-PModes and messages. Inside the certificates folder, you find sample **certificates** that can be used for sending (signing/encrypting) and receiving (verifying) messages.

## Settings

The `settings.xml` located inside the config folder contains several global configuration settings used inside the component. Each kind of setting is explained in the following paragraphs.

### GUID Format

When creating AS4 Messages, Message Ids are being generated. To configure the format in which this must be done the `<IdFormat/>` tag is being used inside the `settings.xml`.

Default: `{GUID}@{IPADDRESS}`

### Payload Service In Process

This setting defines whether or not the (optional) PayloadService should be started in-process with the AS4.NET MessageHandler.

The PayloadService is a REST service that can contain payloads that are referenced by submit-messages.
More information regarding the PayloadService can be found in the Technical Analysis document.

### FE In Process

This setting defines whether or not the AS4.NET FrontEnd should be started in-process with the AS4.NET MessageHandler.
The AS4.NET FrontEnd web-application lets you configure the AS4.NET messagehandler and offers monitoring functionality.

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

| Member name           | Description                                  
| --------------------- | ---------------------------------------------
| AddressBook           | The X.509 certificate store for other users.
| AuthRoot              | The X.509 certificate store for third-party certificate authorities (CAs).
| CertificateAuthority  | The X.509 certificate store for intermediate certificate authorities (CAs).
| Disallowed            | The X.509 certificate store for revoked certificates.
| My                    | The X.509 certificate store for personal certificates.
| Root                  | The X.509 certificate store for trusted root certificate authorities (CAs).
| TrustedPeople         | The X.509 certificate store for directly trusted people and resources.
| TrustedPublisher      | The X.509 certificate store for directly trusted publishers.

### Agents

The AS4 protocol has several operations: Submit, Send, Receive, Deliver and Notify. All of these operations are configured in the settings.xml as Agents. Each agent has three items which defines the agent: **Receiver**, **Transformer** and **Step(s)**.

A **Receiver** can be configured in the `<Receiver/>` tag in each agent. There are multiple kinds of receivers: FileReceiver, DatastoreReceiver, HttpReceiver… Each needed to be configured in order to work correctly. This can be done in the children of this tag as a `<Setting/>` (with attribute **key**; the inner text of the tag is the **value**).

A **Transformer** can be configured in the `<Transformer/>` tag in each agent. This transformer is needed in order to transform the received message (could be `.xml`, `.json`…) and transform it to a AS4 Message that can be used in the Step(s).

A **Step** or **Steps** can be configured in the `<StepConfiguration/>` tag in each agent.  The StepConfiguration element must at least contain a NormalPipeline element.  The **NormalPipeline** element contains the steps that must be executed by the Agent. These steps will be executed after the message is being transformed. Example of steps are: `CreateReceiptStep`, `CompressAttachmentsStep`, `DecryptAS4MessageStep`…

The **StepConfiguration** can contain an **ErrorPipeline** element as well.  This element contains the Steps that will be executed when a step in the **NormalPipeline** failed to execute successfully.

Each tag (**Receiver**, **Transformer** and **Step**) has a **type** attribute which defines the type of which the instance must be created inside the component.

### Custom Settings

When creating custom implementations of types, settings can sometimes be useful. For example, an **Email Sender** by which you configure the **SMTP Server** in the `settings.xml`. This can be useful instead of hardcoded each configured value in the implementation itself.

## Sending Processing Mode

This contract describes all the properties available in the Sending PMode.  The required data fields are marked as mandatory; default values are provided.  Some values of the Sending PMode can be overridden by a SubmitMessage. This definition is available as XSD.

<table class="relative-table wrapped confluenceTable" style="margin-left: 30.0px;width: 70.2424%;">
    <colgroup>
        <col style="width: 24.3577%;">
        <col style="width: 5.50012%;">
        <col style="width: 70.0086%;">
    </colgroup>
    <tbody style="margin-left: 30.0px;">
        <tr style="margin-left: 30.0px;">
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;text-align: center;"><u><strong>Sending PMode</strong></u></p>
            </th>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;text-align: center;" align="center">*</p>
            </th>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;text-align: center;" align="left"><u><strong>Description</strong></u></p>
            </th>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>Id</strong></p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left">PMode Unique Id</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>AllowOverride</strong></p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left">Boolean indication whether a SubmitMessage may override already configured values within the sending PMode.</p>
                <p align="left"><em>Default:</em> false</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>MEP</strong></p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left">Message Exchange Pattern</p>
                <p align="left"><em>Enumeration:</em></p>
                <ul>
                    <li>OneWay</li>
                    <li>TwoWay</li>
                </ul>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>MEPBinding</strong></p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left">Message Exchange Pattern Binding</p>
                <p align="left"><em>Enumeration:</em></p>
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>push</li>
                            <li>pull</li>
                        </ul>
                    </li>
                </ul>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>PushConfiguration</strong></p>
                <p>&nbsp;&nbsp; <em><u>Protocol</u></em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; URL</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; UseChunking</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; UseHTTPCompression</p>
                <p>&nbsp;&nbsp; <em><u>TLSConfiguration</u></em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; IsEnabled</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; TLSVersion</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <br><em><u>ClientCertificateReference</u></em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; ClientCertificateFindType</p>
                <p>ClientCertificateFindValue</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center"><br>O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left">Element must be present when MEPBinding is set to <em>push</em></p>
                <p align="left"><em>&nbsp;</em></p>
                <p align="left">URL of the receiving MSH</p>
                <p align="left"><em>Default: </em>false (true &gt; not implemented)</p>
                <p align="left"><em>Default: </em>false (true &gt; not implemented)</p>
                <p align="left"><br></p>
                <p align="left"><em>Default:</em> false</p>
                <p align="left"><em>Enumeration:</em></p>
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>SSL 3.0</li>
                            <li>TLS 1.0</li>
                            <li>TLS 1.1</li>
                            <li>TLS 1.2</li>
                        </ul>
                    </li>
                </ul>
                <p align="left"><br>Information on how to retrieve the SSL certificate</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left"><strong>DynamicDiscovery</strong></p>
                <p>&nbsp;&nbsp; SmpProfile</p>
                <p>&nbsp;&nbsp; <em>Settings</em></p>
                <p>&nbsp;&nbsp;&nbsp; <em>Setting</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp; Key</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left">This element is only present when SMP/SML is required</p>
                <p align="left">The FQN of the class that implements the IDynamicDiscoveryProfile interface that must be used. If this is not defined, the internal implementation must be used by default.</p>
                <p align="left">Custom settings to configure the IDynamicDiscoveryProfile.</p>
                <p align="left"><br/></p>
                <p align="left"><br/></p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>Reliability</strong></p>
                <p>&nbsp;&nbsp; <em><u>ReceptionAwareness</u></em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; IsEnabled</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; RetryCount</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; RetryInterval</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left"><br></p>
                <p align="left"><br></p>
                <p align="left"><em>Default:</em> false</p>
                <p align="left"><em>Default:</em> 5</p>
                <p align="left"><em>Default:</em> 00:01:00 (HH:mm:ss)</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>ReceiptHandling</strong></p>
                <p>&nbsp;&nbsp; VerifyNRR</p>
                <p>&nbsp;&nbsp; NotifyMessageProducer</p>
                <p>&nbsp;&nbsp; NotifyMethod</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Type</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameters</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameter</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Name</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center"></p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left"><br></p>
                <p align="left"><em>Indicates if Non-Repudiation of Receipt must be verified. Default:</em> true</p>
                <p align="left"><em>Default:</em> false</p>
                <p align="left"><br></p>
                <p align="left">Type of the Notify Agent</p>
                <p align="left">Required parameters for the specified agent</p>
                <p align="left"><br></p>
                <p align="left">Name of the parameter</p>
                <p align="left">Value of the parameter</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>ErrorHandling</strong></p>
                <p>&nbsp;&nbsp; NotifyMessageProducer</p>
                <p>&nbsp;&nbsp; NotifyMethod</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Type</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameters</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameter</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Name</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left"><br></p>
                <p align="left"><em>Default:</em> false</p>
                <p align="left"><br></p>
                <p align="left">Type of the Notify Agent</p>
                <p align="left">Required parameters for the specified agent</p>
                <p align="left"><br></p>
                <p align="left">Name of the parameter</p>
                <p align="left">Value of the parameter</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>ExceptionHandling</strong></p>
                <p>&nbsp;&nbsp; NotifyMessageProducer</p>
                <p>&nbsp;&nbsp; NotifyMethod</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Type</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em><u>Parameters</u></em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameter</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Name</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left"><br></p>
                <p align="left"><em>Default:</em> false</p>
                <p align="left"><br></p>
                <p align="left">Type of the Notify Agent</p>
                <p align="left">Required parameters for the specified agent</p>
                <p align="left"><br></p>
                <p align="left">Name of the parameter</p>
                <p align="left">Value of the parameter</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>Security</strong></p>
                <p><strong>&nbsp;&nbsp; </strong><em><u>Signing</u></em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; IsEnabled</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CertificateFindCriteria</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CertificateFindType</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CertificateFindValue</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; KeyReferenceMethod</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Algorithm</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; HashFunction</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp; <em><u>Encryption</u></em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; IsEnabled</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; PublicKeyCertificate</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Certificate</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CertificateFindCriteria</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CertificateFindType</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p style="margin-left: 60.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</p>
                <p style="margin-left: 60.0px;"><br>CertificateFindValueAlgorithm&nbsp;&nbsp;</p>
                <p><br></p>
                <p><br><br><br></p>
                <p><br></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; AlgorithmKeySize</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; KeyTransport</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; TransportAlgorithm</p>
                <p><br><br></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; DigestAlgorithm</p>
                <p><br><br></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; MgfAlgorithm</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center"><br><br></p>
                <p style="margin-left: 30.0px;" align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p align="center"><br><br><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p align="center"><br><br></p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p align="center"><br><br></p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p align="center"><br></p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left"><br></p>
                <p align="left"><br></p>
                <p align="left"><em>Default:</em> false</p>
                <p align="left"><em>&nbsp;</em></p>
                <p align="left"><em>Enumeration:</em></p>
                <ul>
                    <li>FindByThumbprint</li>
                    <li>FindBySubjectName</li>
                    <li>FindBySubjectDistinguishedName</li>
                    <li>FindByIssuerName</li>
                    <li>FindByIssuerDistinguishedName</li>
                    <li>FindBySerialNumber</li>
                    <li>FindByTimeValid</li>
                    <li>FindByTimeNotValid</li>
                    <li>FindByTimeNotYetValid</li>
                    <li>FindByTimeExpired</li>
                    <li>FindByTemplateName</li>
                    <li>FindByApplicationPolicy</li>
                    <li>FindByCertificatePolicy</li>
                    <li>FindByExtension</li>
                    <li>FindByKeyUsage</li>
                    <li>FindBySubjectKeyIdentifier</li>
                </ul>
                <p align="left"><em>&nbsp;</em></p>
                <p align="left"><em>Enumeration:</em></p>
                <ul>
                    <li>BSTReference <em>(default)</em></li>
                    <li>IssuerSerial</li>
                    <li>KeyIdentifier</li>
                </ul>
                <p align="left"><br><br><br><br></p>
                <p align="left"><em><br></em></p>
                <p align="left"><em>Default</em>: false</p>
                <p align="left"><em>PublicKeyCertificate or CertificateFindCriteria must be specified</em></p>
                <p align="left"><em>Base64 representation of the certificate that must be used</em></p>
                <p align="left"><em>PublicKeyCertificate or CertificateFindCriteria must be specified</em></p>
                <p align="left"><em>Enumeration:</em></p>
                <ul>
                    <li>FindByThumbprint</li>
                    <li>FindBySubjectName</li>
                    <li>FindBySubjectDistinguishedName</li>
                    <li>FindByIssuerName</li>
                    <li>FindByIssuerDistinguishedName</li>
                    <li>FindBySerialNumber</li>
                    <li>FindByTimeValid</li>
                    <li>FindByTimeNotValid</li>
                    <li>FindByTimeNotYetValid</li>
                    <li>FindByTimeExpired</li>
                    <li>FindByTemplateName</li>
                    <li>FindByApplicationPolicy</li>
                    <li>FindByCertificatePolicy</li>
                    <li>FindByExtension</li>
                    <li>FindByKeyUsage</li>
                    <li>FindBySubjectKeyIdentifier</li>
                </ul>
                <p align="left"><em><br>Supported values:</em></p>
                <ul>
                    <li><a href="http://www.w3.org/2009/xmlenc11#aes128-gcm">http://www.w3.org/2009/xmlenc11#aes128-gcm</a></li>
                    <li><a href="http://www.w3.org/2001/04/xmlenc#des-cbc">http://www.w3.org/2001/04/xmlenc#des-cbc</a></li>
                    <li><a href="http://www.w3.org/2001/04/xmlenc#tripledes-cbc">http://www.w3.org/2001/04/xmlenc#tripledes-cbc</a></li>
                    <li><a href="http://www.w3.org/2001/04/xmlenc#aes128-cbc">http://www.w3.org/2001/04/xmlenc#aes128-cbc</a></li>
                    <li><a href="http://www.w3.org/2001/04/xmlenc#aes192-cbc">http://www.w3.org/2001/04/xmlenc#aes192-cbc</a></li>
                    <li><a href="http://www.w3.org/2001/04/xmlenc#aes256-cbc">http://www.w3.org/2001/04/xmlenc#aes256-cbc</a></li>
                </ul>
                <p align="left"><em>Supported values: 128, 192, 256.&nbsp; Default is 128</em></p>
                <p align="left"><br></p>
                <p align="left"><em>Supported values:</em></p>
                <ul>
                    <li><a href="http://www.w3.org/2009/xmlenc11#rsa-oaep">http://www.w3.org/2009/xmlenc11#rsa-oaep</a><em> (default)</em></li>
                    <li><a href="http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p">http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p</a></li>
                </ul>
                <p align="left"><em>Supported values: </em></p>
                <ul>
                    <li><a href="http://www.w3.org/2000/09/xmldsig#sha1">http://www.w3.org/2000/09/xmldsig#sha1</a><em> </em></li>
                    <li><a href="http://www.w3.org/2001/04/xmlenc#sha256">http://www.w3.org/2001/04/xmlenc#sha256</a> <em>(default)</em></li>
                </ul>
                <p align="left"><em>Supported values:</em></p>
                <ul>
                    <li><a href="http://www.w3.org/2009/xmlenc11#mgf1sha1">http://www.w3.org/2009/xmlenc11#mgf1sha1</a> (default)</li>
                    <li><a href="http://www.w3.org/2009/xmlenc11#mgf1sha224">http://www.w3.org/2009/xmlenc11#mgf1sha224</a></li>
                    <li><a href="http://www.w3.org/2009/xmlenc11#mgf1sha256">http://www.w3.org/2009/xmlenc11#mgf1sha256</a></li>
                    <li><a href="http://www.w3.org/2009/xmlenc11#mgf1sha384">http://www.w3.org/2009/xmlenc11#mgf1sha384</a></li>
                    <li><a href="http://www.w3.org/2009/xmlenc11#mgf1sha512">http://www.w3.org/2009/xmlenc11#mgf1sha512</a></li>
                </ul>
                <p align="left">Note that the Mgf Algorithm cannot be specified when the TransportAlgorithm is not set to &nbsp;<a href="http://www.w3.org/2009/xmlenc11#rsa-oaep">http://www.w3.org/2009/xmlenc11#rsa-oaep</a></p>
                <p align="left"><br></p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <p><strong>Message Packaging</strong></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp; Mpc</p>
                <p style="margin-left: 30.0px;">&nbsp; UseAS4Compression</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp; IsMultiHop</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp; IncludePModeId</p>
                <p style="margin-left: 30.0px;"><strong>&nbsp;&nbsp; </strong><u>PartyInfo</u></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>FromParty</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyIds</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyId</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Id</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Role</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>ToParty</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyIds</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyId</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Id</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Role</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp; <u>CollaborationInfo</u></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>AgreementRef</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Service</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Action</p>
                <p style="margin-left: 30.0px;"><strong>&nbsp;&nbsp; </strong><u>MessageProperties</u></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>MessageProperty</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Name</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><br></p>
                <p><em>Default: </em><a href="http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/defaultMPC">http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/defaultMPC</a></p>
                <p><em>Default: false</em></p>
                <p><em>Default: false</em></p>
                <p><em>Default: false</em></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p>Id of the sending party</p>
                <p>Type of Id of the sending party</p>
                <p>Role of the sending party</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p>Id of the receiving party</p>
                <p>Type of Id of the receiving party</p>
                <p>Role of the receiving party</p>
                <p><br></p>
                <p><br></p>
                <p>Information about the partner agreement</p>
                <p><br></p>
                <p>Type of the agreement reference</p>
                <p><br></p>
                <p>The name of the service that is consumed</p>
                <p>Type of the service</p>
                <p>The service operation that is consumed</p>
                <p><br></p>
                <p><br></p>
                <p>Name of the message property</p>
                <p>Type of the message property</p>
                <p align="left">Value of the message property</p>
            </td>
        </tr>
    </tbody>
</table>

 (*): M = Mandatory | O = Optional | R = Recommended

 ## Receiving PMode

 This contract describes all the properties available in the Receiving PMode.  The required data fields are marked as mandatory; default values are provided.  This definition is available as XSD.

<table class="wrapped relative-table confluenceTable" style="margin-left: 30.0px;width: 70.5455%;">
    <colgroup>
        <col style="width: 19.2293%;">
        <col style="width: 5.54991%;">
        <col style="width: 75.041%;">
    </colgroup>
    <tbody style="margin-left: 30.0px;">
        <tr style="margin-left: 30.0px;">
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;text-align: center;"><u><strong>Receive PMode</strong></u></p>
            </th>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">*</p>
            </th>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;text-align: center;" align="left"><u><strong>Description</strong></u></p>
            </th>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>Id</strong></p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p>PMode Unique Id</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>Reliability</strong></p>
                <p>&nbsp;&nbsp; <em>DuplicateElimination</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; IsEnabled</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><br></p>
                <p><br></p>
                <p><em>Default:</em> false</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>ReplyHandling</strong></p>
                <p>&nbsp;&nbsp; ReplyPattern</p>
                <p><br><br></p>
                <p>&nbsp;&nbsp; SendingPMode</p>
                <p>&nbsp;&nbsp; <strong>ReceiptHandling</strong></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; UseNRRFormat</p>
                <p><strong>&nbsp;&nbsp; ErrorHandling</strong></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; UseSOAPFault</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; ResponseHttpCode</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center"><br>M</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><br></p>
                <p><em>Enumeration:</em></p>
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>Response: sync response (<em>default</em>)</li>
                            <li>Callback: async response</li>
                        </ul>
                    </li>
                </ul>
                <p>Reference to the Sending PMode</p>
                <p><br></p>
                <p>Specifies if NonRepudationInfo must be included in receipt.&nbsp; <em>Default: false</em></p>
                <p><em>Default: false</em></p>
                <p>HTTP Status Code in case of reply = response.&nbsp; <em>Default: 200</em></p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>ExceptionHandling</strong></p>
                <p>&nbsp;&nbsp; NotifyMessageConsumer</p>
                <p>&nbsp;&nbsp; NotifyMethod</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Type</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameters</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameter</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Name</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><br></p>
                <p><em>Default:</em> false</p>
                <p><br></p>
                <p>Type of the Notify Agent</p>
                <p>Required parameters for the specified agent</p>
                <p><br></p>
                <p>Name of the parameter</p>
                <p>Value of the parameter</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>Security</strong></p>
                <p><strong>&nbsp;&nbsp; </strong><em>SigningVerification</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Signature</p>
                <p><br></p>
                <p><br></p>
                <p><br>&nbsp;&nbsp; <em>Decryption</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Encryption</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CertificateFindCriteria</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CertificateFindType</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CertificateFindValue</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center"><br>O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p style="margin-left: 30.0px;">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><br></p>
                <p><br></p>
                <p><em>Enumeration</em></p>
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>Allowed (<em>default)</em></li>
                            <li>Not allowed</li>
                            <li>Required</li>
                            <li>Ignored<br><br></li>
                        </ul>
                    </li>
                </ul>
                <p><em>Enumeration</em></p>
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>Ignored (<em>default)</em></li>
                            <li>Allowed</li>
                            <li>Not allowed</li>
                            <li>Required</li>                            
                        </ul>
                    </li>
                </ul>
                <p><em><br></em></p>
                <p><em>Enumeration:</em></p>
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>FindByThumbprint</li>
                            <li>FindBySubjectName</li>
                            <li>FindBySubjectDistinguishedName</li>
                            <li>FindByIssuerName</li>
                            <li>FindByIssuerDistinguishedName</li>
                            <li>FindBySerialNumber</li>
                            <li>FindByTimeValid</li>
                            <li>FindByTimeNotValid</li>
                            <li>FindByTimeNotYetValid</li>
                            <li>FindByTimeExpired</li>
                            <li>FindByTemplateName</li>
                            <li>FindByApplicationPolicy</li>
                            <li>FindByCertificatePolicy</li>
                            <li>FindByExtension</li>
                            <li>FindByKeyUsage</li>
                            <li>FindBySubjectKeyIdentifier</li>
                        </ul>
                    </li>
                </ul>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <p><strong>Message Packaging</strong></p>
                <p>&nbsp;&nbsp; <u>PartyInfo</u></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>FromParty</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyIds</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyId</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Id</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Role</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>ToParty</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyIds</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyId</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Id</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Role</p>
                <p>&nbsp;&nbsp; <u>CollaborationInfo</u></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>AgreementRef</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Service</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Action</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p>Id of the sending party</p>
                <p>Type of Id of the sending party</p>
                <p>Role of the sending party</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p>Id of the receiving party</p>
                <p>Type of Id of the receiving party</p>
                <p>Role of the receiving party</p>
                <p><br></p>
                <p><br></p>
                <p>Information about the partner agreement</p>
                <p>Type of the agreement reference</p>
                <p><br></p>
                <p>The name of the service that is consumed</p>
                <p>Type of the service</p>
                <p>The service operation that is consumed</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>MessageHandling</strong></p>
                <p><strong><strong>&nbsp; </strong></strong><u>Deliver</u></p>
                <p><strong>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Deliver</strong></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;IsEnabled</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;PayloadReferenceMethod</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Type</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameters</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameter</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Name</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;DeliverMethod</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Type</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameters</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameter</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Name</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
                <p><span>&nbsp; </span><u>Forward</u></p>
                <p><span>&nbsp;&nbsp;&nbsp;&nbsp; SendingPMode</span></p>
                <p style="margin-left: 30.0px;"><strong><br></strong></p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center"><span>O</span></p>
                <p style="margin-left: 30.0px;" align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center"><span>O</span></p>
                <p style="margin-left: 30.0px;" align="center"><span>M</span></p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p>Describes how a received ebMS Message must be handled</p>
                <p><span>Deliver or Forward must be specified, not both</span></p>
                <p><em><br></em></p>
                <p><em>True</em> or <em>false</em></p>
                <p>Payload Deliver method (HTTP, FILE(**)…)</p>
                <p>Required parameters</p>
                <p><br></p>
                <p><br></p>
                <p>Name of the parameter</p>
                <p>Value of the parameter</p>
                <p>Type of the Deliver method (HTTP, FILE,..)</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p>Name of the parameter</p>
                <p>Value of the parameter</p>
                <p>Deliver or Forward must be specified, not both</p>
                <p><span>The name of the PMode that must be used to forward the received Message.</span></p>
            </td>
        </tr>
    </tbody>
</table>

(*): M = Mandatory | O = Optional | R = Recommended

(**) When the received payloads must be delivered to the FileSystem, the following parameters available:

- **Location**: 
  The location on the filesystem (directory) where the payloads must be delivered.

- **FileNameFormat**: 
  Defines how the filename of the delivered payloads must look like. There are two macro's available that can be used to define this pattern:
  - `{MessageId}` : inserts the ebMS MessageId in the filename
  - `{AttachmentId}` : inserts the AttachmentId in the filename
    It is possible to combine the macro's which means that it is possible to use `{MessageId}_{AttachmentId}`.

    When the `FileNameFormat` parameter is not defined, the AttachmentId of the payload will be used as the filename
    When the `FileNameFormat` parameter is defined, but it contains none of the above defined parameters, then `_{AttachmentId}` will be appended.

    > (The FileNameFormat parameter is available as from AS4.NET v2.0.1)

- **AllowOverwrite**: 
  Defines whether files with the same name can be overwritten when delivering a payload.  
  Possible values are True and False, the default-value is false.

  > (The AllowOverwrite parameter is available as from AS4.NET v2.0.1)


