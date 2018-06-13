# Receive Agent

The following section describes what the **Receive Agent** is and what its responsibility is:

![receive agent](images/receive-agent.png)

## Agent Responsibility

The **Receive Agent** is a "connection-point" to other MSH instances. This agent will receive `AS4Message`'s that are being sent form other MSH's.
During the receive operation, the incoming `AS4Message` will be verified if it has a valid signature, if the possible attachments are correctly encrypted, ... All these verification options (and others) are configured in a _Receiving Processing Mode_.

## Message Flow

When an `AS4Message` is received by the agent it goes through a series of steps:

1.  Determine the _Receiving Processing Mode_ that must be used to correctly process the incoming message.  
Each configured _Receiving Processing Mode_ is inspected and assigned a score.  The _Receiving Processing Mode_ that best matches the received `AS4Message` will be used to further process the `AS4Message`.

    > IMPORTANT: If none of the configured _Receiving Processing Modes_ sufficiently match the received `AS4Message`, or if it is impossible to select just one matching _Receiving Processing Mode_, then the Agent will stop processing the received `AS4Message` right here since it is not possible to correctly process the message.  
    In this case, the **Receive Agent** will respond with an `Error` signalmessage.

2.  When a _Receiving Processing Mode_ is selected, the received message  will be validated. The validation consists of the following rules:

    - The SOAP body of the `AS4Message` should be empty
    - Each `ParttInfo` element should have an unique `href` reference to a embedded payload (`href` that starts with `'cid:'`)
    - Each included `Attachment` should reference a `PartInfo`

3.  After the validation, the included `Attachment`'s (if any) will be decrypted using the information found in the assigned _Receiving Processing Mode_ (decryption algorithm that must be used, the certificate that must be used to decrypt, ...).  
Decryption only takes place if this is allowed by the pmode: this means that the `Decryption` option must be set to `Required` or `Allowed`.  
When the `Decryption` option is set to `Ignored`, the received message will not be decrypted.  When `Decryption` is set to `NotAllowed` the **Receive Agent** will respond with an `Error`signalmessage if the message is encrypted.
    
4.  After the message is possibly decrypted, the Agent verifies if the signature (if present) is valid.  
This is only done when the `Signature` option in the pmode is set to `Required` or `Allowed`.  
In the case of `Ignored` or `NotAllowed` the verification will not take place or isn't allowed at all.

    > Note: for incoming Non-Repudiation `Receipt`'s, the _Receiving Processing Mode_ has an option (`VerifyNRR`) to also verify the included references.

5.  After the signature has been verified, the included `Attachments`'s (if any) will be decompressed.

    > Note: if the incoming message must be forwarded, the attachments will not be decompressed.

6.  Finally, when the message has been successfully processed, the receive agent will create a `Receipt` signalmessage.  This `Receipt` will be created conforming the settings that can be found in the _Receiving Processing mode_.  (`ReceiptHandling.UseNRRFormat` to specify if NRI information must be included and the `Signing.IsEnabled` setting in the responding pmode (which is defined by the `ReplyHandling.SendingPMode` element)). This `Receipt` will then be sent to the sender.
Next to that, the **Receive Agent** will make sure that the received `AS4Message` is delivered or forwarded, depending on what is configured in the `MessageHandling` section in the _Receiving Processing Mode_.

  If any of the above described steps fail for any reason: not matched _Receiving Processing Mode_, decryption failure, signature invalid, ... an `Error` will be created that includes the failure message. This `Error` will be signed if the _Receiving Processing Mode_ is configured this way (`Signing.IsEnabled` is set to `true`).

## Agent Trigger

The **Receive** operation is triggered each time the configured _Receiver_ receives an `AS4Message`. This _Receiver_ will act as the "entry-point" of the agent. If no `AS4Message` is received, the **Receive** operation will be triggered but the agent will respond with an error saying an unexpected message is received.

## Static Receive Agent

The **Receive Agent** can be configured as a **Static Receive Agent**. This requires you to pre-configure the **Receive Agent** with a specifiec _Receiving Processing Mode_. A **Static Receive Agent** will only allow _UserMessages_ to be received, _SignalMessages_ will be rejected.
For each received `AS4Message`, the specified _Receiving Processing Mode_ will be used during the **Receive Operation** and no determination of a _Receiving Processing Mode_ will take place.

To configure a **Static Receive Agent**, do the following:

- Go to the configuration of the **Receive Agent** you want to make static.
- Make sure the **Transformer** is a `ReceiveMessageTransformer`
- The setting `ReceivingPMode` will be appear
- Specify the _Receiving Processing Mode_ by selecting one of the configured pmodes
