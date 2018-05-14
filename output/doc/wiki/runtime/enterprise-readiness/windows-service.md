# Windows Service

The <b>AS4.NET</b> component can be installed as Windows Service. This page expands more on this topic; and how it's different from running it as a console-application.

## Installation

The Service can be installed by running the `.bat` files located in the `.\service-setup\` folder at the root of the package.
This folder contains two installation files:

* `install-windows-service.bat`
* `uninstall-windows-service.bat`

**Both script files needs to be executed as Administrator; because installing a Windows Service requires those permissions!**

After the service is correctly installed, you can run it from the **Windows Services Control Manager**; we're it's called: **<b>AS4.NET</b> Message Service Handler**.

## Configuration

Like the Console Host, the Windows Service uses a configuration file to initialize itself. This file is called `settings-service.xml` and is located in the `.\config\` folder.

By default, the **<b>AS4.NET</b> Portal** and the **Payload Service** are started in-process when the <b>AS4.NET</b> messagehandler starts.

**Both the Console Host and the Windows Service listens to the same HTTP ports; so they can't run next to each other with the default settings.**

## Logging

When running the service, the log messages are written to **Event Log**:

![img](images/eventlog.png)