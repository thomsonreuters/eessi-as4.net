import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { TlsConfiguration } from './TlsConfiguration';
import { ClientCertificateReferenceForm } from './ClientCertificateReferenceForm';

export class TlsConfigurationForm {
    public static getForm(formBuilder: FormWrapper, current: TlsConfiguration | undefined = undefined): FormWrapper {
        let form = formBuilder
            .group({
                [TlsConfiguration.FIELD_isEnabled]: [!!(current && current.isEnabled)],
                [TlsConfiguration.FIELD_tlsVersion]: [(current == null || current.tlsVersion == null) ? 3 : current.tlsVersion],
                [TlsConfiguration.FIELD_clientCertificateReference]: ClientCertificateReferenceForm.getForm(formBuilder.subForm(TlsConfiguration.FIELD_clientCertificateReference), current && current.clientCertificateReference).form,
            })
            .onChange<boolean>(TlsConfiguration.FIELD_isEnabled, (isEnabled, wrapper) => {
                if (isEnabled) {
                    wrapper.form!.get(TlsConfiguration.FIELD_tlsVersion)!.enable();
                    wrapper.form!.get(TlsConfiguration.FIELD_clientCertificateReference)!.enable();
                } else {
                    wrapper.form!.get(TlsConfiguration.FIELD_tlsVersion)!.disable();
                    wrapper.form!.get(TlsConfiguration.FIELD_clientCertificateReference)!.disable();
                }
            })
            .triggerHandler(TlsConfiguration.FIELD_isEnabled, current && current.isEnabled);

        return form;
    }
}
