# Pull Receive Agent

The following section describes what the **Pull Receive Agent** is and what's its responsibility is:

![pull receive agent](images/pull-receive-agent.png)

## Agent Responsibility

The **Pull Receive Agent** has the same responsibility as the (Push) **Receive Agent**: to receive `AS4Message`'s via the configured _Receiver_ from a sending MSH. Instead of receiving messages directly, the **Pull Receive Agent** polls the sender for an available messages.

Since the **Pull Receive Agent** polls for available messages on the sending MSH, this agent takes control of when an AS4 Message is received instead of making sure that there's constantly a receive-endpoint available.
The **Pull Receive Agent** is useful in situations where the receiving MSH isn't always online or has a limited bandwith.

## Message Flow

The `AS4Message`'s that are received by the **Pull Receive Agent** are processed in exactly the same way as the (Push) **Receive Agent** would handle them.
See the message flow section in the (Push) **Receive Agent** documentation for more information on how a received `AS4Message`is processed.

## Agent Trigger

The  **Pull Receive Agent** initiates the receive process by sending a _PullRequest_ signal message to the sending MSH.  When the sending MSH receives such a _PullRequest_, the sender responds with an `AS4Message` that contains a `UserMessage`.  If the sending MSH does not have any `UserMessage`s available for the received `PullRequest`, the sender responds with a special `Error` message that indicates that there are no `UserMessage`s available.

The _PullRequest_ signal messages are sent by the **Pull Receive Agent** using an _Exponential Backoff_ algorithm: 

- when the sending MSH responds with a _no messages available_ message, the _Receiver_ will wait a bit longer to poll for the next `AS4Message`.  
- when the sending MSH responds with a `UserMessage`, the _Receiver_ will reset the polling interval and will immediately sent a new _PullRequest_.

> The minimum and maximum polling interval used in the _Exponential Backoff_ is configurable on the _Receiver_.

## Message Partition Channels

It is possible to specify that only certain `UserMessage`s may be received.  This is done by specifying the **Message Partition Channel** or **MPC** to which the `UserMessage`s that are being polled for, must belong to.

The **MPC** that must be used is defined in the PMode that is configured on the _Receiver_ of the **Pull Receive Agent**.  If no **MPC** is specified in the PMode, the default **MPC** (h<span>ttp://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/defaultMPC</span>) will be used.