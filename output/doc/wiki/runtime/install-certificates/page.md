# Install Certificates

This procedure describes how to install certificates.

- Click **Start → Run**
- Type _mmc_ and click Open

![mmc](mmc.png)

- Choose **File → Add/Remove Snap-In**
- Select **Certificates** and click **Add**
- Choose **Computer Account** and click **Next**

![cert-snapin](cert-snapin.png)

- Keep **Local Computer** selected and click **Finish**

![select-computer](select-computer.png)

- Click **OK**

![add-remove-snapin](add-remove-snapin.png)

- Right click on the correct certificate store and choose **All Tasks → Import**
- Click **Next**

![import-cert](import-cert.png)

- Click **Browse**
- To import a public certificate, you can select the `*.cer` certificate.
- To import a private certificate, select the `*.pfx` certificate

- In case of a private certificate (`.pfx`):
    - Specify the password (see `samples\certificates\README.txt`)
    - Mark the certificate as exportable

![export-cert](export-cert.png)

- Click **Next**, **Next** and **Finish**