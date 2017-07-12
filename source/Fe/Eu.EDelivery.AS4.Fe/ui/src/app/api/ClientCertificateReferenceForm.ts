import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { ClientCertificateReference } from './ClientCertificateReference';
import { thumbPrintValidation } from '../validators/thumbprintValidator';

export class ClientCertificateReferenceForm {
    public static getForm(formBuilder: FormWrapper, current: ClientCertificateReference | undefined): FormWrapper {
        let form = formBuilder
            .group({
                clientCertificateFindType: [current && current.clientCertificateFindType, Validators.required],
                clientCertificateFindValue: [current && current.clientCertificateFindValue, [Validators.required, thumbPrintValidation]]
            })
            .onChange<number>(ClientCertificateReference.FIELD_clientCertificateFindValue, (selected, wrapper) => {
                let findValue = wrapper.form.get(ClientCertificateReference.FIELD_clientCertificateFindValue)!;
                findValue.clearValidators();
                if (+selected === 0) {
                    findValue.setValidators([Validators.required, thumbPrintValidation]);
                } else {
                    findValue.setValidators(Validators.required);
                }
            })
            .onStatusChange(undefined, (status, wrapper) => {
                if (status !== 'DISABLED' && !!!wrapper.form.get(ClientCertificateReference.FIELD_clientCertificateFindType)!.value) {
                    wrapper.form.get(ClientCertificateReference.FIELD_clientCertificateFindType)!.setValue(0);
                }
            })
            .triggerHandler(ClientCertificateReference.FIELD_clientCertificateFindValue, current && current.clientCertificateFindValue);
        return form;
    }
}
