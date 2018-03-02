# Enterprise Readiness

This section expands on the enterprise usage of the AS4.NET component.

* [Install Configure Reverse Proxy](install-configure-reverse-proxy.md)
* [Enable HTTPS without IIS](enable-https-without-iis.md)
* [Install Certificates](install-certificates.md)
* [Install as Windows Service](windows-servivce.md)
* [Configure Logging](configure-logging.md)

## Multiple Instances on the Same Datstore

For these situations, we recommand to use **SQL Server** as datastore system; because of its cabability to lock on row level when several queries are made to the same table.

> NOTE: this is only supported starting from v3.0