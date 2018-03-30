# Configure Logging

The AS4 .NET Component uses NLog for logging. The logging levels can be configured to minimum: **Fatal**, and maximum: **Trace**. Further configuration can be found on the NLog support itself (https://github.com/nlog/nlog/wiki/Configuration-file )

The **NLog** configuration can be found in the `Eu.EDelivery.AS4.ServiceHandler.ConsoleHost.config` file.

![app-config](images/app.config.png)

When there are issues with signature-verification of a received AS4 Message, it is possible to enable extensive logging which can be helpful in pinpointing the problem.
Signature verification logging can be enabled by enabling the `XmlDsigLogSwitch` trace-switch that can be found in the `App.config` configuration file:

![app-config-details](images/app.config-detail.png)

