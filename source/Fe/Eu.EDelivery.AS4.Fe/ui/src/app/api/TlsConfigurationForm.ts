import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TlsConfiguration } from './TlsConfiguration';
import { ClientCertificateReferenceForm } from './ClientCertificateReferenceForm';

export class TlsConfigurationForm {
    public static getForm(formBuilder: FormBuilder, current: TlsConfiguration): FormGroup {
        let form = formBuilder.group({
            [TlsConfiguration.FIELD_isEnabled]: [!!(current && current.isEnabled)],
            [TlsConfiguration.FIELD_tlsVersion]: [(current == null || current.tlsVersion == null) ? 3 : current.tlsVersion],
            [TlsConfiguration.FIELD_clientCertificateReference]: ClientCertificateReferenceForm.getForm(formBuilder, current && current.clientCertificateReference),
        });
        TlsConfigurationForm.setupForm(form);
        return form;
    }
    private static setupForm(form: FormGroup) {
        this.processEnabled(form);
        form.get(TlsConfiguration.FIELD_isEnabled).valueChanges.subscribe(() => this.processEnabled(form));
    }
    private static processEnabled(form: FormGroup) {
        let isEnabled = form.get(TlsConfiguration.FIELD_isEnabled).value;
        if (isEnabled) {
            form.get(TlsConfiguration.FIELD_tlsVersion).enable();
            form.get(TlsConfiguration.FIELD_clientCertificateReference).enable();
        } else {
            form.get(TlsConfiguration.FIELD_tlsVersion).disable();
            form.get(TlsConfiguration.FIELD_clientCertificateReference).disable();
        }
    }
}
