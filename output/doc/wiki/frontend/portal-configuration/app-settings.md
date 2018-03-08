<table class="relative-table confluenceTable" style="width: 83.7989%;">
    <colgroup>
        <col style="width: 22.7652%;">
        <col style="width: 77.1741%;">
    </colgroup>
    <tbody>
        <tr>
            <th class="confluenceTh">
                <p><strong>Setting name</strong></p>
            </th>
            <th class="confluenceTh">
                <p><strong>Description</strong></p>
            </th>
        </tr>
        <tr>
            <td class="confluenceTd">
                <p>Port</p>
            </td>
            <td class="confluenceTd">
                <p>This defines the IP address &amp; port on which the portal should be listening. For ex if you set this to <a href="http://127.0.0.1:9000/">http://127.0.0.1:9000</a>, then the portal will be reachable using a browser at <a href="http://127.0.0.1:9000">http://127.0.0.1:9000</a>.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <p>Logging</p>
            </td>
            <td class="confluenceTd">
                <p>This block is used to configure the internal .NET logging. Since these are settings not specific to the AS4 portal we refer to the Microsoft documentation for this <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging#log-filtering">https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging#log-filtering</a></p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <p><strong>Settings</strong></p>
            </td>
            <td class="confluenceTd">
                <p>This section contains settings used for the portal.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>ShowStackTraceInExceptions</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>If true, then a stack trace will be shown in the error dialog when something went something unexpected wrong. This is will contain the server exception!<br>For production, it’s best to set this option to false, to avoid leaking sensitive
                    information.
                </p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>SettingsXml</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>This option indicates where the settings.xml used by the runtime is located.<br>Default setting is ‘./config/settings.xml’. You can also specify a UNC path.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>Runtime</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>The location where all the runtime files are located (DLL’s / exe).<br>Default is ‘.’.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <p><strong>Authentication</strong></p>
            </td>
            <td class="confluenceTd">
                <p>Section for configure the authentication of the portal</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>Provider</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>The database provider used for authentication.<br>Default is ‘sqlite’, another possible option id ‘mssql’.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>ConnectionString</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>The connection string to the authentication database.<br>Default is ‘FileName=users.sqlite’, when setting the provider to ‘mssql’ then a mssql connectionstring is expected here.<br>You can look at <a href="https://www.connectionstrings.com/sql-server/">https://www.connectionstrings.com/sql-server/</a>                    for examples of mssql connection strings.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>JwtOptions.Issuer</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>A string indicating who issued the JWT token. This can be any string.<br>Default is “AS4.NET”.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>JwtOptions.Audience</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>A string indicating for which services this JWT token is. This can also be any string.<br>Default is “<a href="http://localhost:5000">http://localhost:5000</a>”.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>JwtOptions.ValidFor</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>The amount of days that the JWT token is valid.<br>Default is 1 day</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>JwtOptions.Key</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>The secret key used to sign the JWT token.<br>Default is “auf0i3VsjznH6K6imUl2”, but we strongly advice to change this key! This can be configured when the portal is started for the first time!</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <p><strong>Monitor</strong></p>
            </td>
            <td class="confluenceTd">
                <p>Section containing settings for the monitor module.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>Provider</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>The database provider used to access the runtime database.<br>Default is “sqlite”, another option is “mssql”.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>ConnectionString</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>The connection string for the runtime database.</p>
                <p>Default is “FileName=users.sqlite”, when setting the provider to ‘mssql’ then a mssql connectionstring is expected here.<br>You can look at <a href="https://www.connectionstrings.com/sql-server/">https://www.connectionstrings.com/sql-server/</a>                    for examples of mssql connection strings.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <p><strong>Pmodes</strong></p>
            </td>
            <td class="confluenceTd">
                <p>All configuration settings to find PModes.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>SendingPmodeFolder</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>The folder containing all the sending PModes. This can also be a UNC path.</p>
                <p>Default is “./config/send-pmodes”.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>ReceivingPmodeFolder</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>The folder containing all the receiving PModes. This can also be a UNC path.;</p>
                <p>Default is “./config/receive-pmodes”.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <p><strong>SubmitTool</strong></p>
            </td>
            <td class="confluenceTd">
                <p>This section contains all the settings used in the test message tool.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>ToHttpAddress</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>The MSH endpoint to send messages to.</p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>PayloadHttpAddress</li>
                        </ul>
                    </li>
                </ul>
            </td>
            <td class="confluenceTd">
                <p>The HTTP endpoint of a payload service to send payloads to.</p>
            </td>
        </tr>
    </tbody>
</table>