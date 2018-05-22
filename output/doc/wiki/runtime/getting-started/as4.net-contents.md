# <b>AS4.NET</b> Contents

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

Inside the logs folder, you can find detailed debug and error logs. In order to modify the log configuration, follow the instructions in the section: _"Configure Logging"_.

### Messages Folder

Inside the messages folder, following structure is created:

* **attachments**
* **errors**
* **exceptions**
* **receipts**
* **in**
* **out**

The **attachments** folder contains several files (pictures and .xml documents) that are being used as reference for the send AS4 messages. The **receipts/errors/exceptions** folders are used to store **Notify Messages**. The in folder is used to store incoming messages and attachments; the **out** folder is being used to send messages to another MSH (the .xml file will be renamed to `.accepted` if it’s being retrieved by the component).

These folders are used just to get started with the component. The component can be configured to use other file folders.

### Samples Folder

Inside the samples folder, following structure is created:

* **certificates**
* **messages**
* **receive-pmodes**
* **send-pmodes**

Each folder contains the respectively the samples of send/receive-PModes and messages. Inside the certificates folder, you find sample **certificates** that can be used for sending (signing/encrypting) and receiving (verifying) messages.
