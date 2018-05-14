# Enable HTTPS Without IIS

This procedure describes how to enable HTTPS for <b>AS4.NET</b> agents that use a HttpReceiver (ReceiveAgent, SubmitAgent, PullReceiveAgent)

## SSL Certificates

To be able to use HTTPS, an SSL certificate is required.  Such a certificate can be obtained via a Certificate Authority.

For testing purposes, it is possible to generate such a SSL certificate yourself (self-signed certificate).  In a production environment, an SSL certificate that is issued by a Certificate Authority must be used.

### Obtaining a self-signed SSL certificate

A self-signed SSL certificate can be obtained by executing the following Powershell command:

    PS:\> New-SelfSignedCertificate -CertStoreLocation Cert:\LocalMachine\My -DnsName <domain name>

The above command will generate a self-signed SSL Certificate and put it in the **Personal** Windows certificate store on the local machine.

### Installing SSL Certificate

On the receiving side, the SSL certificate must be installed in de **Personal** certificate store of the Local Machine.  Both the public and the private key must be present.
When using a self-signed certificate, the self-signed certificate must also be present in the Trusted Root Certification Authorities certificate store.

Parties that are sending messages to the <b>AS4.NET</b> MSH, must use the public key of the SSL certificate that is configured on the server.

### Enabling HTTPS

The final step to enable https for <b>AS4.NET</b>, is to bind the SSL certificate to the IP address and port that the <b>AS4.NET</b> MSH is using.
This can be done by executing the command below in a command prompt window that has elevated rights:

        C:\> netsh http add sslcert ipport=0.0.0.0:8433 certhash=0110d7ec0e9fe7f3801f9761837cac29469b37d8 appid={40e359ee-b4b8-4a4a-b8fe-db2240df097e}

- `ipport`: the IP address and port that is used by the <b>AS4.NET</b> agent. Specifying `0.0.0.0` means 'all ip address on the local machine'.

This example thus assumes that the <b>AS4.NET</b> MSH is configured for https on port 8443.

- `certhash`: the thumbprint of the SSL certificate.  This information can be found using the certificates Management Console
- `appid`: The Guid that represents the TypeLib Id of an application.  For the <b>AS4.NET</b> console-host application, this is `{40e359ee-b4b8-4a4a-b8fe-db2240df097e}`