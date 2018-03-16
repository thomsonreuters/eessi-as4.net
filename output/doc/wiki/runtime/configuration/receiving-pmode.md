<table class="wrapped relative-table confluenceTable" style="margin-left: 30.0px;width: 70.5455%;">
    <colgroup>
        <col style="width: 19.2293%;">
        <col style="width: 5.54991%;">
        <col style="width: 75.041%;">
    </colgroup>
    <tbody style="margin-left: 30.0px;">
        <tr style="margin-left: 30.0px;">
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;text-align: center;"><u><strong>Receive PMode</strong></u></p>
            </th>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">*</p>
            </th>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;text-align: center;" align="left"><u><strong>Description</strong></u></p>
            </th>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>Id</strong></p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p>PMode Unique Id</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>Reliability</strong></p>
                <p>&nbsp;&nbsp; <em>DuplicateElimination</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; IsEnabled</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><br></p>
                <p><br></p>
                <p><em>Default:</em> false</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>ReplyHandling</strong></p>
                <p>&nbsp;&nbsp; ReplyPattern</p>
                <p><br><br></p>
                <p>&nbsp;&nbsp; SendingPMode</p>
                <p>&nbsp;&nbsp; <strong>ReceiptHandling</strong></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; UseNRRFormat</p>
                <p><strong>&nbsp;&nbsp; ErrorHandling</strong></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; UseSOAPFault</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; ResponseHttpCode</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center"><br>M</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><br></p>
                <p><em>Enumeration:</em></p>
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>Response: sync response (<em>default</em>)</li>
                            <li>Callback: async response</li>
                        </ul>
                    </li>
                </ul>
                <p>Reference to the Sending PMode</p>
                <p><br></p>
                <p>Specifies if NonRepudationInfo must be included in receipt.&nbsp; <em>Default: false</em></p>
                <p><em><br></em></p>
                <p><em>Default: false</em></p>
                <p>HTTP Status Code in case of reply = response.&nbsp; <em>Default: 200</em></p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>ExceptionHandling</strong></p>
                <p>&nbsp;&nbsp; NotifyMessageConsumer</p>
                <p>&nbsp;&nbsp; NotifyMethod</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Type</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameters</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameter</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Name</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><br></p>
                <p><em>Default:</em> false</p>
                <p><br></p>
                <p>Type of the Notify Agent</p>
                <p>Required parameters for the specified agent</p>
                <p><br></p>
                <p>Name of the parameter</p>
                <p>Value of the parameter</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>Security</strong></p>
                <p><strong>&nbsp;&nbsp; </strong><em>SigningVerification</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Signature</p>
                <p><br></p>
                <p><br></p>
                <p><br>&nbsp;&nbsp; <em>Decryption</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Encryption</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CertificateFindCriteria</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CertificateFindType</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CertificateFindValue</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center"><br>O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p style="margin-left: 30.0px;">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><br></p>
                <p><br></p>
                <p><em>Enumeration</em></p>
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>Allowed (<em>default)</em></li>
                            <li>Not allowed</li>
                            <li>Required</li>
                            <li>Ignored<br><br></li>
                        </ul>
                    </li>
                </ul>
                <p><em>Enumeration</em></p>
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>Allowed (<em>default)</em></li>
                            <li>Not allowed</li>
                            <li>Required</li>
                            <li>Ignored</li>
                        </ul>
                    </li>
                </ul>
                <p><em><br></em></p>
                <p><em>Enumeration:</em></p>
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>FindByThumbprint</li>
                            <li>FindBySubjectName</li>
                            <li>FindBySubjectDistinguishedName</li>
                            <li>FindByIssuerName</li>
                            <li>FindByIssuerDistinguishedName</li>
                            <li>FindBySerialNumber</li>
                            <li>FindByTimeValid</li>
                            <li>FindByTimeNotValid</li>
                            <li>FindByTimeNotYetValid</li>
                            <li>FindByTimeExpired</li>
                            <li>FindByTemplateName</li>
                            <li>FindByApplicationPolicy</li>
                            <li>FindByCertificatePolicy</li>
                            <li>FindByExtension</li>
                            <li>FindByKeyUsage</li>
                            <li>FindBySubjectKeyIdentifier</li>
                        </ul>
                    </li>
                </ul>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <p><strong>Message Packaging</strong></p>
                <p>&nbsp;&nbsp; <u>PartyInfo</u></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>FromParty</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyIds</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyId</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Id</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Role</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>ToParty</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyIds</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyId</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Id</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Role</p>
                <p>&nbsp;&nbsp; <u>CollaborationInfo</u></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>AgreementRef</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Service</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Action</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p>Id of the sending party</p>
                <p>Type of Id of the sending party</p>
                <p>Role of the sending party</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p>Id of the receiving party</p>
                <p>Type of Id of the receiving party</p>
                <p>Role of the receiving party</p>
                <p><br></p>
                <p><br></p>
                <p>Information about the partner agreement</p>
                <p>Type of the agreement reference</p>
                <p><br></p>
                <p>The name of the service that is consumed</p>
                <p>Type of the service</p>
                <p>The service operation that is consumed</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>MessageHandling</strong></p>
                <p><strong><strong>&nbsp; </strong></strong><u>Deliver</u></p>
                <p><strong>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Deliver</strong></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;IsEnabled</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;PayloadReferenceMethod</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Type</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameters</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameter</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Name</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;DeliverMethod</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Type</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameters</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameter</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Name</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
                <p><span>&nbsp; </span><u>Forward</u></p>
                <p><span>&nbsp;&nbsp;&nbsp;&nbsp; SendingPMode</span></p>
                <p style="margin-left: 30.0px;"><strong><br></strong></p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center"><span>O</span></p>
                <p style="margin-left: 30.0px;" align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center"><span>O</span></p>
                <p style="margin-left: 30.0px;" align="center"><span>M</span></p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p>Describes how a received ebMS Message must be handled</p>
                <p><span>Deliver or Forward must be specified, not both</span></p>
                <p><em><br></em></p>
                <p><em>True</em> or <em>false</em></p>
                <p>Payload Deliver method (HTTP, FILE(**)…)</p>
                <p>Required parameters</p>
                <p><br></p>
                <p><br></p>
                <p>Name of the parameter</p>
                <p>Value of the parameter</p>
                <p>Type of the Deliver method (HTTP, FILE,..)</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p>Name of the parameter</p>
                <p>Value of the parameter</p>
                <p>Deliver or Forward must be specified, not both</p>
                <p><span>The name of the PMode that must be used to forward the received Message.</span></p>
            </td>
        </tr>
    </tbody>
</table>