# AS4.NET
AS4.NET is an open-source application that implements the OASIS AS4 specification. It supports both the e-SENS e-Delivery and the EESSI AS4 Messaging profile as an ebMS endpoint.   
 
The component has been conformance tested against the e-SENS eDelivery specifications.  

AS4.NET is interoperable with multiple other AS4 gateway providers; AS4.NET has undergone performance and interop-tests against Holodeck B2B, RSSBus, Domibus, Flame Message Server and IBM B2B.

Basic testing against the EESSI AS4 Messaging profile is already covered. Extensive testing is scheduled later this year.

#Documentation
A configuration- and usermanual for AS4.NET can be found [online]([https://ec.europa.eu/cefdigital/wiki/display/EDELCOMMUNITY/AS4.NET)

# Features
- One-Way/Push message exchange pattern (v1.0)
- XML based configuration (v1.0)
- XML based PMode configuration (v1.0)
- Dynamic PMode override (v1.0)
- Multiple submit, notify and deliver agents (v1.0)
- FILE based receivers and senders (v1.0)
- Signing and encryption using WS-Security (v1.0)
- AS4 Compression (v1.0)
- AS4 Reception Awareness and Retry (v1.0)
- AS4 Duplicate Detection and Elimination (v1.0)

- Submit, deliver and notify via HTTP protocol (v1.1) 
- Submit and deliver attachments via HTTP protocol (v1.1)
- One-Way/Pull pattern as initiator (v1.1)
- Support for sub channels in One-Way/Pull pattern (v1.1)
- Support for multi-hop AS4 profile (v1.1)
- Support for TLS client certificates (v1.1)
- Performance tuning for large messages, up to 2GB (v1.1)
- Performance tuning for high volume processing (v1.1) 

- Web interface for configuration (v2.0)
- Web interface for monitoring (v2.0)
- Web interface for user management (v2.0)
- Web interface for testing (v2.0)
- One-Way/Pull pattern as responder (v2.0)
- Support for sub-channels (v2.0)
- Support for message forwarding (v2.0)
- Support for MEP bridging (v2.0)
- Support for PullRequest authorization (v2.0)
- Support for SMP/SML dynamic discovery (v2.0)
- Support for TLS server side (v2.0)
- Continued performance tuning for large messages, up to 2GB (v2.0)
- Continued performance tuning for high volume processing (v2.0)
- Improvements to the internal messaging engine (v2.0)
 

# Installation
AS4.NET v2.0.0 can be downloaded from [the following location](https://ec.europa.eu/cefdigital/artifact/content/repositories/eDelivery/eu/eessi/as4/eessi_as4.net/1.1.0/eessi_as4.net-1.1.0.zip). The documentation on how to use and configure AS4.NET can be found in the documentation folder of the package.

# Third Party software
The following third party libraries are used by AS4.NET:
- [Automapper](https://github.com/AutoMapper/AutoMapper) ([MIT License](https://opensource.org/licenses/MIT))
- [BouncyCastle](https://github.com/bcgit/bc-csharp) ([MIT License](https://opensource.org/licenses/MIT))
- [FluentValidation](https://github.com/JeremySkinner/FluentValidation) ([Apache 2](http://www.apache.org/licenses/LICENSE-2.0.html))
- [Mimekit](https://github.com/jstedfast/MimeKit) ([MIT License](https://opensource.org/licenses/MIT))
- [Nlog](https://github.com/NLog/NLog) ([BSD License](https://opensource.org/licenses/BSD-3-Clause))
- [Polly](https://github.com/App-vNext/Polly) ([BSD License](https://opensource.org/licenses/BSD-3-Clause))
- [SQLite](https://sqlite.org/) ([Public Domain](https://sqlite.org/copyright.html))
- [Remotion](https://github.com/re-motion/Relinq) ([Apache 2](http://www.apache.org/licenses/LICENSE-2.0.html))

# License
This software is licensed under the [EUPL License v1.1](https://joinup.ec.europa.eu/community/eupl/og_page/european-union-public-licence-eupl-v11).
