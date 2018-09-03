# Custom Connector

## Introduction

The implementation of <b>AS4.NET</b> can be expanded by writing custom connectors. This document explains how these custom connectors can be created to extend the <b>AS4.NET</b> component with some custom functionality (delivery of messages to other locations, retrieve payloads that must be sent from other locations, receive messages from custom sources, …).

- [Custom Receiver](custom-receiver.md)
- [Custom Attachment Uploading](custom-attachment-uploading.md)
- [Custom Delivery](custom-delivery.md)

### <b>AS4.NET</b> Components

The <b>AS4.NET</b> component consists of different Agents, each with a different responsibility: sending, receiving, forwarding, notifying, delivering, … Each Agent inside the <b>AS4.NET</b> component has three major items: **Receiver**, **Transformer**, and **Steps**.

![send-receive-agent](./images/send-receive-agent.png)

The **Receiver** is the first component inside an **Agent** that takes the initiative. It receives messages that the **Agent** can process. When a message is received by the **Receiver** it will first be transformed by a **Tranformer** before it goes through one or many **Steps**. The Transformer will make sure that the end result is a canonical-format message so we can reuse **Steps** across **Agents**. The **Steps** will adapt the message, update external sources, and finaly send the result to the target that needs the message; this can be another **Agent** or a external target.
