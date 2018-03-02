<table class="relative-table wrapped confluenceTable" style="margin-left: 30.0px;width: 70.2424%;">
    <colgroup>
        <col style="width: 24.3577%;">
        <col style="width: 5.50012%;">
        <col style="width: 70.0086%;">
    </colgroup>
    <tbody style="margin-left: 30.0px;">
        <tr style="margin-left: 30.0px;">
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;text-align: center;"><u><strong>Sending PMode</strong></u></p>
            </th>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;text-align: center;" align="center">*</p>
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
                <p align="left">PMode Unique Id</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>AllowOverride</strong></p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left">Boolean indication whether a SubmitMessage may override already configured values within the sending PMode.</p>
                <p align="left"><em>Default:</em> false</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>MEP</strong></p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left">Message Exchange Pattern</p>
                <p align="left"><em>Enumeration:</em></p>
                <ul>
                    <li>OneWay</li>
                    <li>TwoWay</li>
                </ul>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>MEPBinding</strong></p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left">Message Exchange Pattern Binding</p>
                <p align="left"><em>Enumeration:</em></p>
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>push</li>
                            <li>pull</li>
                        </ul>
                    </li>
                </ul>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>PushConfiguration</strong></p>
                <p>&nbsp;&nbsp; <em><u>Protocol</u></em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; URL</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; UseChunking</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; UseHTTPCompression</p>
                <p>&nbsp;&nbsp; <em><u>TLSConfiguration</u></em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; IsEnabled</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; TLSVersion</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <br><em><u>ClientCertificateReference</u></em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; ClientCertificateFindType</p>
                <p>ClientCertificateFindValue</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center"><br>O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left">Element must be present when MEPBinding is set to <em>push</em></p>
                <p align="left"><em>&nbsp;</em></p>
                <p align="left">URL of the receiving MSH</p>
                <p align="left"><em>Default: </em>false (true &gt; not implemented)</p>
                <p align="left"><em>Default: </em>false (true &gt; not implemented)</p>
                <p align="left"><br></p>
                <p align="left"><em>Default:</em> false</p>
                <p align="left"><em>Enumeration:</em></p>
                <ul>
                    <li style="list-style-type: none;background-image: none;">
                        <ul>
                            <li>SSL 3.0</li>
                            <li>TLS 1.0</li>
                            <li>TLS 1.1</li>
                            <li>TLS 1.2</li>
                        </ul>
                    </li>
                </ul>
                <p align="left"><br>Information on how to retrieve the SSL certificate</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left"><strong>DynamicDiscovery</strong></p>
                <p>&nbsp;&nbsp; SmlScheme</p>
                <p>&nbsp;&nbsp; SmpServerDomainName</p>
                <p>&nbsp;&nbsp; DocumentIdentifier</p>
                <p>&nbsp;&nbsp; DocumentIdentifierScheme</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left">This element is only present when SMP/SML is required</p>
                <p align="left">Used to build the SML Uri. <em>Default</em>: iso6523-actorid-upis</p>
                <p align="left">Domain name that must be used in the Uri</p>
                <p align="left">Used to retrieve the correct DocumentIdentifier.</p>
                <p align="left"><em>Default:</em>busdox-docid-qns</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>Reliability</strong></p>
                <p>&nbsp;&nbsp; <em><u>ReceptionAwareness</u></em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; IsEnabled</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; RetryCount</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; RetryInterval</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left"><br></p>
                <p align="left"><br></p>
                <p align="left"><em>Default:</em> false</p>
                <p align="left"><em>Default:</em> 5</p>
                <p align="left"><em>Default:</em> 00:01:00 (HH:mm:ss)</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>ReceiptHandling</strong></p>
                <p>&nbsp;&nbsp; NotifyMessageProducer</p>
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
                <p align="left"><br></p>
                <p align="left"><em>Default:</em> false</p>
                <p align="left"><br></p>
                <p align="left">Type of the Notify Agent</p>
                <p align="left">Required parameters for the specified agent</p>
                <p align="left"><br></p>
                <p align="left">Name of the parameter</p>
                <p align="left">Value of the parameter</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>ErrorHandling</strong></p>
                <p>&nbsp;&nbsp; NotifyMessageProducer</p>
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
                <p align="left"><br></p>
                <p align="left"><em>Default:</em> false</p>
                <p align="left"><br></p>
                <p align="left">Type of the Notify Agent</p>
                <p align="left">Required parameters for the specified agent</p>
                <p align="left"><br></p>
                <p align="left">Name of the parameter</p>
                <p align="left">Value of the parameter</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>ExceptionHandling</strong></p>
                <p>&nbsp;&nbsp; NotifyMessageProducer</p>
                <p>&nbsp;&nbsp; NotifyMethod</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Type</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em><u>Parameters</u></em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Parameter</em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Name</p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value</p>
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
                <p align="left"><br></p>
                <p align="left"><em>Default:</em> false</p>
                <p align="left"><br></p>
                <p align="left">Type of the Notify Agent</p>
                <p align="left">Required parameters for the specified agent</p>
                <p align="left"><br></p>
                <p align="left">Name of the parameter</p>
                <p align="left">Value of the parameter</p>
            </td>
        </tr>
        <tr style="margin-left: 30.0px;">
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p><strong>Security</strong></p>
                <p><strong>&nbsp;&nbsp; </strong><em><u>Signing</u></em></p>
                <p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; IsEnabled</p>
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
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CertificateFindValue</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; KeyReferenceMethod</p>
                <p><br></p>
                <p><br></p>
                <p><br></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Algorithm</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; HashFunction</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp; <em><u>Encryption</u></em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; IsEnabled</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; PublicKeyCertificate</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Certificate</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CertificateFindCriteria</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; CertificateFindType</p>
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
                <p style="margin-left: 60.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</p>
                <p style="margin-left: 60.0px;"><br>CertificateFindValueAlgorithm&nbsp;&nbsp;</p>
                <p><br></p>
                <p><br><br><br></p>
                <p><br></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; AlgorithmKeySize</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; KeyTransport</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; TransportAlgorithm</p>
                <p><br><br></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; DigestAlgorithm</p>
                <p><br><br></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; MgfAlgorithm</p>
            </td>
            <th style="margin-left: 30.0px;" class="confluenceTh">
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
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
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">M</p>
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
                <p style="margin-left: 30.0px;" align="center"><br><br></p>
                <p style="margin-left: 30.0px;" align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center">M</p>
                <p align="center"><br><br><br></p>
                <p align="center"><br></p>
                <p align="center"><br></p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p align="center"><br><br></p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p align="center"><br><br></p>
                <p style="margin-left: 30.0px;" align="center">O</p>
                <p align="center"><br></p>
            </th>
            <td style="margin-left: 30.0px;" class="confluenceTd">
                <p align="left"><br></p>
                <p align="left"><br></p>
                <p align="left"><em>Default:</em> false</p>
                <p align="left"><em>&nbsp;</em></p>
                <p align="left"><em>Enumeration:</em></p>
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
                <p align="left"><em>&nbsp;</em></p>
                <p align="left"><em>Enumeration:</em></p>
                <ul>
                    <li>BSTReference</li>
                    <li>IssuerSerial</li>
                    <li>KeyIdentifier</li>
                </ul>
                <p align="left"><br><br><br><br></p>
                <p align="left"><em><br></em></p>
                <p align="left"><em>Default</em>: false</p>
                <p align="left"><em>PublicKeyCertificate or CertificateFindCriteria must be specified</em></p>
                <p align="left"><em>Base64 representation of the certificate that must be used</em></p>
                <p align="left"><em>PublicKeyCertificate or CertificateFindCriteria must be specified</em></p>
                <p align="left"><em>Enumeration:</em></p>
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
                <p align="left"><em><br>Supported values:</em></p>
                <ul>
                    <li><a href="http://www.w3.org/2009/xmlenc11#aes128-gcm">http://www.w3.org/2009/xmlenc11#aes128-gcm</a></li>
                    <li><a href="http://www.w3.org/2001/04/xmlenc#des-cbc">http://www.w3.org/2001/04/xmlenc#des-cbc</a></li>
                    <li><a href="http://www.w3.org/2001/04/xmlenc#tripledes-cbc">http://www.w3.org/2001/04/xmlenc#tripledes-cbc</a></li>
                    <li><a href="http://www.w3.org/2001/04/xmlenc#aes128-cbc">http://www.w3.org/2001/04/xmlenc#aes128-cbc</a></li>
                    <li><a href="http://www.w3.org/2001/04/xmlenc#aes192-cbc">http://www.w3.org/2001/04/xmlenc#aes192-cbc</a></li>
                    <li><a href="http://www.w3.org/2001/04/xmlenc#aes256-cbc">http://www.w3.org/2001/04/xmlenc#aes256-cbc</a></li>
                </ul>
                <p align="left"><em>Supported values: 128, 192, 256.&nbsp; Default is 128</em></p>
                <p align="left"><br></p>
                <p align="left"><em>Supported values:</em></p>
                <ul>
                    <li><a href="http://www.w3.org/2009/xmlenc11#rsa-oaep">http://www.w3.org/2009/xmlenc11#rsa-oaep</a><em> (default)</em></li>
                    <li><a href="http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p">http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p</a></li>
                </ul>
                <p align="left"><em>Supported values: </em></p>
                <ul>
                    <li><a href="http://www.w3.org/2000/09/xmldsig#sha1">http://www.w3.org/2000/09/xmldsig#sha1</a><em> </em></li>
                    <li><a href="http://www.w3.org/2001/04/xmlenc#sha256">http://www.w3.org/2001/04/xmlenc#sha256</a> <em>(default)</em></li>
                </ul>
                <p align="left"><em>Supported values:</em></p>
                <ul>
                    <li><a href="http://www.w3.org/2009/xmlenc11#mgf1sha1">http://www.w3.org/2009/xmlenc11#mgf1sha1</a> (default)</li>
                    <li><a href="http://www.w3.org/2009/xmlenc11#mgf1sha224">http://www.w3.org/2009/xmlenc11#mgf1sha224</a></li>
                    <li><a href="http://www.w3.org/2009/xmlenc11#mgf1sha256">http://www.w3.org/2009/xmlenc11#mgf1sha256</a></li>
                    <li><a href="http://www.w3.org/2009/xmlenc11#mgf1sha384">http://www.w3.org/2009/xmlenc11#mgf1sha384</a></li>
                    <li><a href="http://www.w3.org/2009/xmlenc11#mgf1sha512">http://www.w3.org/2009/xmlenc11#mgf1sha512</a></li>
                </ul>
                <p align="left">Note that the Mgf Algorithm cannot be specified when the TransportAlgorithm is not set to &nbsp;<a href="http://www.w3.org/2009/xmlenc11#rsa-oaep">http://www.w3.org/2009/xmlenc11#rsa-oaep</a></p>
                <p align="left"><br></p>
            </td>
        </tr>
        <tr>
            <td class="confluenceTd">
                <p><strong>Message Packaging</strong></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp; Mpc</p>
                <p style="margin-left: 30.0px;">&nbsp; UseAS4Compression</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp; IsMultiHop</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp; IncludePModeId</p>
                <p style="margin-left: 30.0px;"><strong>&nbsp;&nbsp; </strong><u>PartyInfo</u></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>FromParty</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyIds</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyId</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Id</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Role</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>ToParty</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyIds</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>PartyId</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Id</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Role</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp; <u>CollaborationInfo</u></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>AgreementRef</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>Service</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Action</p>
                <p style="margin-left: 30.0px;"><strong>&nbsp;&nbsp; </strong><u>MessageProperties</u></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <em>MessageProperty</em></p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Name</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Type</p>
                <p style="margin-left: 30.0px;">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Value</p>
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
                <p><em>Default: </em><a href="http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/defaultMPC">http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/defaultMPC</a></p>
                <p><em>Default: false</em></p>
                <p><em>Default: false</em></p>
                <p><em>Default: false</em></p>
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
                <p><br></p>
                <p>Type of the agreement reference</p>
                <p><br></p>
                <p>The name of the service that is consumed</p>
                <p>Type of the service</p>
                <p>The service operation that is consumed</p>
                <p><br></p>
                <p><br></p>
                <p>Name of the message property</p>
                <p>Type of the message property</p>
                <p align="left">Value of the message property</p>
            </td>
        </tr>
    </tbody>
</table>