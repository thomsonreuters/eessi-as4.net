## Remove Sending PMode as responding PMode

Since <span>AS4.NET</span> vXXX the **Receiving PMode** is used to respond to AS4 messages instead of referencing a **Sending PMode**.
This means that upgrading to this version with older **Receiving PModes** will lead to different responses. The breaking change means that the:

- **Sending PMode**'s `PushConfiguration` becomes **Receiving PMode**'s `ReplyHandling.ResponseConfiguration`
- **Sending PMode**'s `Security.Signing` becomes **Receiving PMode**'s `ReplyHandling.ResponseSigining`

Starting from this version, the release package has a PowerShell script file located at: `.\scripts\copy-responsepmode-to-receivingpmode.ps1` to help you make this change more gracefully.

The script requires two folder paths:

- `sendingPModePath` : folder path to where your **Sending PModes** are located.
- `receivingPModePath` : folder path to where your **Receiving PModes** are located.

```powershell
PS> .\copy-responsepmode-to-receivingpmode.ps1 -sendingPModePath ".\send-pmodes" -receivingPModePath ".\receive-pmodes"
```

After executing the script, all your **Receiving PModes** are updated to use in this version.

### Manual removal of Sending PMode as responding PMode

When the script doesn't work for you, or you can't execute PowerShell scripts on your machine you can always manually change your **Receiving PModes**.

By following these steps you should be able to manually change your **Receiving PModes**:

1. Open a **Receiving PMode** you want to upgrade
2. Locate the `<ReplyHandling/>` element
   1. This element should have a `<SendingPMode/>` element with the Id of the PMode
3. Open the **Sending PMode** with the Id that was referenced in this element
4. Locate the `<PushConfiguration/>` element in the **Sending PMode**
5. Copy the entire element and past it in the `<ReplyHandling/>` element in the **Receiving PMode**
6. Rename the `<PushConfiguration/>` in your **Receiving PMode** to `<ResponseConfiguration/>`
7. Locate the `<Signing/>` element in the **Sending PMode**
8. Copy the entire element in the **Receiving PMode**
9. Rename the `<Signing/>` element in your **Receiving PMode** to `<ResponseSigning/>`
