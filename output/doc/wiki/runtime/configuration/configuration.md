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

<table>
    <tbody>
        <tr>
            <th align="left"><b>Sending PMode</b></th>
            <th align="center">*</th>
            <th align="left">Description</b></u></th>
        </tr>
        <tr>
            <td><b>Id</b></td>
            <td align="center">M</td>
            <td>PMode Unique Id</td>
        </tr>
        <tr>
            <td><b>AllowOverride</b></td>
            <td align="center">O</td>
            <td>
                <div style="width:550px;">Boolean indication whether a SubmitMessage may override already configured values within the sending PMode. <br/> <i>Default:</i> false</div>
            </td>
        </tr>
        <tr>
            <td><b>MEP</b></td>
            <td align="center">M</td>
            <td>
                Message Exchange Pattern
                <i>Enumeration:</i>
                <ul style="margin:0;">
                    <li>OneWay</li>
                    <li>TwoWay</li>
                </ul>
            </td>
        </tr>
        <tr>
            <td><b>MEPBinding</b></td>
            <td align="center">M</td>
            <td>
                Message Exchange Pattern Binding
                <i>Enumeration:</i>
                <ul style="margin:0;">
                    <li>push</li>
                    <li>pull</li>
                </ul>
            </td>
        </tr>
        <tr>
            <td>
                <b>PushConfiguration</b><br/>
                &nbsp;&nbsp;<i><u>Protocol</u></i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;URL<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;UseChunking<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;UseHTTPCompression<br/>
                &nbsp;&nbsp;<i><u>TLSConfiguration</u></i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;IsEnabled<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;TLSVersion<br/>
                <br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<br/><i><u>ClientCertificateReference</u></i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;ClientCertificateFindType<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;ClientCertificateFindValue
            </td>
            <td align="center">
                O<br/>
                M<br/>
                M<br/>
                O<br/>
                O<br/>
                O<br/>
                M<br/>
                M<br/>
                <br/><br/><br/><br/>
                M<br/>
                M<br/>
            </td>
            <td>
                Element must be present when MEPBinding is set to <i>push</i><br/>
                &nbsp;URL of the receiving MSH<br/>
                <i>Default: </i>false (true &gt; not implemented)<br/>
                <i>Default: </i>false (true &gt; not implemented)<br/><br/>
                <i>Default:</i> false<br/>
                <i>Enumeration:</i>
                <ul style="margin:0;">
                    <li>SSL 3.0</li>
                    <li>TLS 1.0</li>
                    <li>TLS 1.1</li>
                    <li>TLS 1.2</li>
                </ul>
                Information on how to retrieve the SSL certificate<br/><br/><br/>
            </td>
        </tr>
        <tr>
            <td>
                <b>DynamicDiscovery</b><br/>
                &nbsp;&nbsp;SmpProfile<br/><br/>
                &nbsp;&nbsp;<i>Settings</i><br/>
                &nbsp;&nbsp;&nbsp;<i>Setting</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;Key<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
            </td>
            <td>
                O<br/>
                O<br/><br/>
                O<br/>
                O<br/>
                M<br/>
                M<br/>
            </td>
            <td>
                This element is only present when SMP/SML is required<br/>
                The FQN of the class that implements the IDynamicDiscoveryProfile interface that must be used. If this is not defined, the internal implementation must be used by default.<br/>
                Custom settings to configure the IDynamicDiscoveryProfile.
                <br/><br/><br/><br/>
            </td>
        </tr>
        <tr>
            <td>
                <b>Reliability</b><br/>
                &nbsp;&nbsp; <i><u>ReceptionAwareness</u></i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;IsEnabled<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;RetryCount<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;RetryInterval<br/>
            </td>
            <td>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
            </td>
            <td>
                <br/><br/>
                <i>Default:</i> false<br/>
                <i>Default:</i> 5<br/>
                <i>Default:</i> 00:01:00 (HH:mm:ss)<br/>
            </td>
        </tr>
        <tr>
            <td>
                <b>ReceiptHandling</b><br/>
                &nbsp;&nbsp;VerifyNRR<br/><br/>
                &nbsp;&nbsp;NotifyMessageProducer<br/>
                &nbsp;&nbsp;NotifyMethod<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Type</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Parameters</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Parameter</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
            </td>
            <td>
                O<br/>
                O<br/><br/>
                O<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
            </td>
            <td>
                <br/>
                Indicates if Non-Repudiation of Receipt must be verified.<br/><i>Default:</i> true<br/>
                <i>Default:</i> false<br/><br/>
                Type of the Notify Agent<br/>
                Required parameters for the specified agent<br/><br/>
                Name of the parameter<br/>
                Value of the parameter<br/>
            </td>
        </tr>
        <tr>
            <td>
                <b>ErrorHandling</b><br/>
                &nbsp;&nbsp;NotifyMessageProducer<br/>
                &nbsp;&nbsp;NotifyMethod<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Type</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Parameters</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Parameter</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value
            </td>
            <td>
                O<br/>
                O<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
            </td>
            <td>
                <br/><i>Default:</i> false<br/><br/>
                Type of the Notify Agent<br/>
                Required parameters for the specified agent<br/><br/>
                Name of the parameter<br/>
                Value of the parameter<br/>
            </td>
        </tr>
        <tr>
            <td>
                <b>ExceptionHandling</b><br/>
                &nbsp;&nbsp;NotifyMessageProducer<br/>
                &nbsp;&nbsp;NotifyMethod<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Type</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i><u>Parameters</u></i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Parameter</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name<br/>
                &nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
            </td>
            <td>
                O<br/>
                O<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
            </td>
            <td>
                <br/><i>Default:</i> false<br/><br/>
                Type of the Notify Agent<br/>
                Required parameters for the specified agent<br/><br/>
                Name of the parameter<br/>
                Value of the parameter<br/>
            </td>
        </tr>
        <tr>
            <td>
                <b>Security</b><br/>
                <b>&nbsp;&nbsp;</b><i><u>Signing</u></i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;IsEnabled<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindCriteria<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindType
                <br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindValue<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;KeyReferenceMethod
                <br/><br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Algorithm<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;HashFunction<br/>
                &nbsp;&nbsp;<i><u>Encryption</u></i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;IsEnabled<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;PublicKeyCertificate<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Certificate<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindCriteria<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindType
                <br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindValue
                <br/>
                Algorithm<br/><br/><br/><br/><br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;AlgorithmKeySize<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;KeyTransport<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;TransportAlgorithm<br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;DigestAlgorithm<br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;MgfAlgorithm<br/>
                <br/><br/><br/>
                <br/><br/><br/><br/>
            </td>
            <td>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                M<br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
                M<br/>
                M<br/><br/><br/><br/>
                M<br/>
                M<br/>
                O<br/>
                O<br/>
                O<br/>
                M<br/>
                O<br/>
                M<br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
                M<br/>
                M<br/><br/><br/><br/><br/><br/><br/>
                O<br/>
                O<br/>
                O<br/><br/><br/>
                O<br/><br/><br/>
                O<br/><br/>
                <br/><br/><br/>
                <br/><br/><br/>
            </td>
            <td>
                <br/><br/>
                <i>Default:</i> false<br/>
                <i>&nbsp;</i><br/>
                <i>Enumeration:</i><br/>
                <ul style="margin:0;">
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
                </ul><br/><br/>
                <i>Enumeration:</i>
                <ul style="margin:0;">
                    <li>BSTReference <i>(default)</i></li>
                    <li>IssuerSerial</li>
                    <li>KeyIdentifier</li>
                </ul>
                <br/><br/><br/>
                <i>Default</i>: false<br/>
                PublicKeyCertificate or CertificateFindCriteria must be specified<br/>
                Base64 representation of the certificate that must be used<br/>
                PublicKeyCertificate or CertificateFindCriteria must be specified<br/>
                <i>Enumeration:</i>
                <ul style="margin:0;">
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
                <i><br/>Supported values:</i>
                <ul style="margin:0;">
                    <li><a>http://www.w3.org/2009/xmlenc11#aes128-gcm</a></li>
                    <li><a>http://www.w3.org/2001/04/xmlenc#des-cbc</a></li>
                    <li><a>http://www.w3.org/2001/04/xmlenc#tripledes-cbc</a></li>
                    <li><a>http://www.w3.org/2001/04/xmlenc#aes128-cbc</a></li>
                    <li><a>http://www.w3.org/2001/04/xmlenc#aes192-cbc</a></li>
                    <li><a>http://www.w3.org/2001/04/xmlenc#aes256-cbc</a></li>
                </ul>
                <i>Supported values:</i> 128, 192, 256.&nbsp; Default is 128<br/><br/>
                <i>Supported values:</i>
                <ul style="margin:0;">
                    <li><a>http://www.w3.org/2009/xmlenc11#rsa-oaep</a> <i>(default)</i></li>
                    <li><a>http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p</a></li>
                </ul>
                <i>Supported values:</i>
                <ul style="margin:0;">
                    <li><a>http://www.w3.org/2000/09/xmldsig#sha1</a><i> </i></li>
                    <li><a>http://www.w3.org/2001/04/xmlenc#sha256</a> <i>(default)</i></li>
                </ul>
                <i>Supported values:</i>
                <ul style="margin:0;">
                    <li><a>http://www.w3.org/2009/xmlenc11#mgf1sha1</a> <i>(default)</i></li>
                    <li><a>http://www.w3.org/2009/xmlenc11#mgf1sha224</a></li>
                    <li><a>http://www.w3.org/2009/xmlenc11#mgf1sha256</a></li>
                    <li><a>http://www.w3.org/2009/xmlenc11#mgf1sha384</a></li>
                    <li><a>http://www.w3.org/2009/xmlenc11#mgf1sha512</a></li>
                </ul>
                Note that the Mgf Algorithm cannot be specified when the TransportAlgorithm is not set to <a>http://www.w3.org/2009/xmlenc11#rsa-oaep</a><br/>
            </td>
        </tr>
        <tr>
            <td>
                <b>Message Packaging</b><br/>
                &nbsp;&nbsp;Mpc<br/>
                &nbsp;&nbsp;UseAS4Compression<br/>
                &nbsp;&nbsp;IsMultiHop<br/>
                &nbsp;&nbsp;IncludePModeId<br/>
                &nbsp;&nbsp;<u>PartyInfo</u><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>FromParty</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>PartyIds</i>
                <br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;PartyId</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Id<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Type<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Role<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>ToParty</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>PartyIds</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;PartyId</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Id<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Type<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Role<br/>
                &nbsp;&nbsp;<u>CollaborationInfo</u><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>AgreementRef</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Type<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>Service</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Type<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Action<br/>
                &nbsp;&nbsp;<u>MessageProperties</u><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>MessageProperty</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Type<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
            </td>
            <td>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
            </td>
            <td>
                <br/>
                <i>Default: </i><a>http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/defaultMPC</a><br/>
                <i>Default:</i> false</i><br/>
                <i>Default:</i> false</i><br/>
                <i>Default:</i> false</i><br/>
                <br/><br/><br/><br/>
                Id of the sending party<br/>
                Type of Id of the sending party<br/>
                Role of the sending party<br/>
                <br/><br/><br/>
                Id of the receiving party<br/>
                Type of Id of the receiving party<br/>
                Role of the receiving party<br/><br/>
                Information about the partner agreement<br/>
                <br/>
                Type of the agreement reference<br/>
                <br/>
                The name of the service that is consumed<br/>
                Type of the service<br/>
                The service operation that is consumed<br/>
                <br/><br/>
                Name of the message property<br/>
                Type of the message property<br/>
                align="left">Value of the message property<br/>
            </td>
        </tr>
    </tbody>
</table>

 (*): M = Mandatory | O = Optional | R = Recommended

 ## Receiving PMode

 This contract describes all the properties available in the Receiving PMode.  The required data fields are marked as mandatory; default values are provided.  This definition is available as XSD.

<table>
    <tbody>
        <tr>
            <th align="left"><b>Receive PMode</b></th>
            <th align="center">*</th>
            <th align="left"><b>Description</b></th>
        </tr>
        <tr>
            <td><b>Id</b></td>
            <td align="center">M</td>
            <td>PMode Unique Id</td>
        </tr>
        <tr>
            <td>
                <b>Reliability</b><br/>
                &nbsp;&nbsp;<i>DuplicateElimination</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;IsEnabled
            </td>
            <td align="center">
                O<br/>
                O<br/>
                O<br/>
            </td>
            <td><br/><br/><i>Default:</i> false</td>
        </tr>
        <tr>
            <td>
                <b>ReplyHandling</b><br/>
                    &nbsp;&nbsp;ReplyPattern<br/><br/><br/>
                    &nbsp;&nbsp;SendingPMode<br/>
                    &nbsp;&nbsp;<b>ReceiptHandling</b><br/>
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;UseNRRFormat<br/><br/>
                    <b>&nbsp;&nbsp;ErrorHandling</b><br/>
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;UseSOAPFault<br/>
                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;ResponseHttpCode<br/>
            </td>
            <td align="center">
                M<br/>
                M<br/><br/><br/>
                M<br/>
                O<br/>
                O<br/><br/>
                O<br/>
                O<br/>
                O<br/>
            </td>
            <td>
            <br/>
                <i>Enumeration:</i>
                <ul style="margin:0;">
                    <li>Response: sync response (<i>default</i>)</li>
                    <li>Callback: async response</li>
                </ul>
                Reference to the Sending PMode <br/><br/>
                <div>Specifies if NonRepudationInfo must be included in receipt <i>Default: false</i></div>
                <i>Default: false</i><br/>
                <br/>HTTP Status Code in case of reply = response. <br/><i>Default: 200</i><br/>
            </td>
        </tr>
        <tr>
            <td>
                <b>ExceptionHandling</b><br/>
                &nbsp;&nbsp; NotifyMessageConsumer<br/>
                &nbsp;&nbsp; NotifyMethod <br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Type</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Parameters</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Parameter</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value
            </td>
            <td align="center">
                O<br/>
                O<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
            </td>
            <td>
                <br/>
                <i>Default:</i> false
                <br/><br/>
                <div style="width:500px;">Type of the Notify Agent Required parameters for the specified agent</div>
                <br/><br/>
                Name of the parameter<br/>
                Value of the parameter
            </td>
        </tr>
        <tr>
            <td>
                <b>Security</b><br/>
                &nbsp;&nbsp;<i>SigningVerification</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Signature<br/><br/><br/><br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;AllowUnknownRootCertificate
                &nbsp;&nbsp;<i>Decryption</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Encryption<br/><br/><br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindCriteria
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindType
                <br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindValue
            </td>
            <td align="center">
                O<br/>
                O<br/>
                M<br/>
                <br/><br/><br/><br/><br/>
                O<br/>
                O<br/>
                O<br/>
                <br/><br/><br/><br/>
                O<br/>
                M<br/>
                <br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
                M<br/>
            </td>
            <td>
                <br/><br/>
                <i>Enumeration</i>
                <ul style="margin:0;">
                    <li>Allowed (<i>default)</i></li>
                    <li>Not allowed</li>
                    <li>Required</li>
                    <li>Ignored</li>
                </ul>
                <br/>
                <div style="width:550px;">Indicates whether certificates with an unknown root authority are trusted. (Default <i>false</i>)</div><br/>               <i>Enumeration:</i>
                <ul style="margin:0;">
                    <li>Ignored (<i>default)</i></li>
                    <li>Allowed</li>
                    <li>Not allowed</li>
                    <li>Required</li>
                </ul>
                <i><br/></i>
                <i>Enumeration:</i>
                <ul style="margin:0;">
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
                <br/>
            </td>
        </tr>
        <tr>
            <td>
                <b>Message Packaging</b><br/>
                &nbsp;&nbsp;<u>PartyInfo</u><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>FromParty</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>PartyIds</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>PartyId</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Id<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Type<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Role<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>ToParty</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>PartyIds</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>PartyId</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Id<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Type<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Role<br/>
                &nbsp;&nbsp;<u>CollaborationInfo</u><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>AgreementRef</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Type<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Service</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Type<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Action<br/>
            </td>
            <td align="center">
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
            </td>
            <td>
                <br/><br/><br/><br/><br/>
                Id of the sending party<br/>
                Type of Id of the sending party<br/>
                Role of the sending party<br/>
                <br/><br/><br/>
                Id of the receiving party<br/>
                Type of Id of the receiving party<br/>
                Role of the receiving party<br/>
                <br/><br/>
                Information about the partner agreement<br/>
                Type of the agreement reference<br/>
                <br/>
                The name of the service that is consumed<br/>
                Type of the service<br/>
                The service operation that is consumed
            </td>
        </tr>
        <tr>
            <td>
                <b>MessageHandling</b><br/>
                <b><b>&nbsp; </b></b><u>Deliver</u><br/>
                <b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Deliver</b><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;IsEnabled<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;PayloadReferenceMethod<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>Type</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>Parameters</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>Parameter</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Name<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;DeliverMethod<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>Type</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>Parameters</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>Parameter</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Name<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value<br/>
                <span>&nbsp; </span><u>Forward</u><br/>
                <span>&nbsp;&nbsp;&nbsp;&nbsp; SendingPMode</span><br/>
            </td>
            <td align="center">
                M<br/>
                O<br/>
                <br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                O<br/>
                M<br/>
            </td>
            <td>
                Describes how a received ebMS Message must be handled<br/>
                Deliver or Forward must be specified, not both<br/>
                <i><br/></i>
                <i>True</i> or <i>false</i>
                Payload Deliver method (HTTP, FILE(**)…)
                Required parameters
                <br/><br/><br/><br/><br/>
                Name of the parameter<br/>
                Value of the parameter<br/><br/>
                Type of the Deliver method (HTTP, FILE,..)<br/>
                <br/><br/>
                Name of the parameter<br/>
                Value of the parameter<br/>
                Deliver or Forward must be specified, not both<br/>
                The name of the PMode that must be used to forward the received Message.
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


