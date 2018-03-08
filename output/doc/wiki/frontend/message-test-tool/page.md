# Message Test Tool

> Note: The configuration for this tool can be changed in the, for this we refer to section “Appsettings.json”.

- The portal contains functionality to create your own AS4 messages. To open the tool, click on the “Test” item in the navigation menu.

    ![portal-test](images/portal-test.png)

- The following UI will appear.

    ![test-ui](images/test-ui.png)

<table class="wrapped relative-table confluenceTable" style="width: 86.4525%;">
    <colgroup>
        <col style="width: 13.6805%;">
        <col style="width: 86.3489%;">
    </colgroup>
    <tbody>
        <tr>
            <th class="confluenceTh">
                <p><strong>Field</strong></p>
            </th>
            <th class="confluenceTh">
                <p><strong>Description</strong></p>
            </th>
        </tr>
        <tr>
            <td class="confluenceTd">
                <p>Sending PMode</p>
            </td>
            <td class="confluenceTd">
                <p>The PMode to be used to send the message.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <p>Payloads</p>
            </td>
            <td class="confluenceTd">
                <div class="content-wrapper">
                    <p>Attachments you want to add to the AS4 message. Adding attachments can be done using the upload icon or by dragging files on the browser window into the following section: _"No payload(s) added yet. Drop your payloads here."_</p>  
                </div>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <p># messages</p>
            </td>
            <td class="confluenceTd">
                <p>Number of messages to generate. Default is 1, maximum is 100.</p>
                <p>When you create multiple messages then the payload urls will be the same for all the messages.</p>
            </td>
        </tr>
    </tbody>
</table>
    
- After the messages are configured, click on the “Send” button and the logging tab will be opened.
- This tab will be updated with real time information about the sending process. From the logging tab, you can also do the following things.

    ![logs](images/logs.png)

    1. will open a dialog showing you the xml of the AS4 message.

        ![pmode-dialog](images/pmode-dialog.png)

    2. “Open message in monitor” will open a new tab showing the message in the monitor.

