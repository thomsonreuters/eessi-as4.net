import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { TlsConfiguration } from './TlsConfiguration';
import { ClientCertificateReferenceForm } from './ClientCertificateReferenceForm';
import { ItemType } from './ItemType';

export class TlsConfigurationForm {
    public static getForm(formBuilder: FormWrapper, current: TlsConfiguration | undefined = undefined, runtime: ItemType[], path: string): FormWrapper {
        let form = formBuilder
            .group({
                [TlsConfiguration.FIELD_isEnabled]: [!!(current && current.isEnabled)],
                [TlsConfiguration.FIELD_tlsVersion]: [formBuilder.createFieldValue(current, TlsConfiguration.FIELD_tlsVersion, path, 0, runtime)],
                [TlsConfiguration.FIELD_clientCertificateReference]: ClientCertificateReferenceForm.getForm(formBuilder.subForm(TlsConfiguration.FIELD_clientCertificateReference), current && current.clientCertificateReference, `${path}.{TlsConfiguration.FIELD_clientCertificateReference}`, runtime).form,
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
            .onStatusChange(undefined, (status, wrapper) => {
                let isEnabled = wrapper.form!.get(TlsConfiguration.FIELD_isEnabled)!.value;
                if (isEnabled && status !== 'DISABLED') {
                    if (!!!wrapper.form!.get(TlsConfiguration.FIELD_tlsVersion)!.value) {
                        wrapper.form!.get(TlsConfiguration.FIELD_tlsVersion)!.setValue(runtime[`${path}.${TlsConfiguration.FIELD_tlsVersion}`]);
                    }
                } else {
                    wrapper.form!.get(TlsConfiguration.FIELD_tlsVersion)!.disable();
                    wrapper.form!.get(TlsConfiguration.FIELD_clientCertificateReference)!.disable();
                }
            })
            .triggerHandler(TlsConfiguration.FIELD_isEnabled, current && current.isEnabled);

        return form;
    }
}
