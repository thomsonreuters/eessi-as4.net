# Submit Agent

The following section describes what the **Submit Agent** is and what its responsibility is:

![submit agent](images/submit-agent.png)

## Agent Responsibility

The **Submit Agent** acts as the "entry-point" of the component for when a message is to be sent. It's only task is to make sure that the `SubmitMessage` is transformed to a `AS4Message` with a _Sending Processing Mode_ so that the message can be send correctly to the next MSH.

## Message Flow

When a `SubmitMessage` gets send to the agent it goes to a flow to transform the message to a canonical AS4 Message used in the rest of the component.

1.  Determine the right _Sending Processing Mode_ used further in the AS4.NET Component
2.  Transform the incoming `SubmitMessage` to a `AS4Message` using both the information specified in the `SubmitMessage` and the determined _Sending Processing Mode_
3.  Send the `AS4Message` to the **Send Agent** so it can further be processed

## Agent Trigger

The **Submit** operation is triggered each time the Business Application (message producer) sends a `SubmitMessage` to the MSH.

## Static Configuration

The **Submit Agent** can be configured as a **Static Submit Agent**. This configuration requires you to submit payloads directly instead of `SubmitMessage`'s. For every submitted payload, the same _Sending Processing Mode_ will be used to create a `SubmitMessage` for you.

To activate this configuration and make it a **Static Submit Agent**, do the following:

* Go to the configuration of the **Submit Agent** you want to make static
* Change the **Transformer** to a `SubmitPayloadTransformer`
  * The setting `SendingPMode` will be appear
* Specify the _Sending Processing Mode_ that must be used to create `SubmitMessage`'s
