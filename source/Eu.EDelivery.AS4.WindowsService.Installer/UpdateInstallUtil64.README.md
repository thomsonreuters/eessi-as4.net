## Update the MSI to install via `InstallUtil64`

We use the default setup projects for visual studio; which means we have to use a 'Custom Action' to install/uninstall a Windows Service via a MSI.
Custom Actions are run in a 32-bit mode which means that internally the `InstallUtil.dll` is used instead of the `InstallUtil64.dll`.
All our 'startup/output' projects are 64-bit which means we get a `BadImageFormatException` if we execute the MSI directly.

This `.mst` file: `UpdateInstallUtil64.mst` script is a transformation-recording that modifies a MSI file to use the `InstallUtil64.dll` file.
This script can be run via the following command in the **Visual Studio Developer Command Prompt**.

> Make sure that you have installed the **MSI Tools** in the [Windows 10 SDK](https://developer.microsoft.com/en-us/windows/downloads/windows-10-sdk)

```
C:\>  msitran -a UpdateInstallUtil64.mst Eu.EDelivery.AS4.WindowsService.Installer.msi
```

After executing this command, the MSI binary table is updated. The MSI file is now ready to be installed.
