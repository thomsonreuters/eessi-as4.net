import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { ClientCertificateReference } from './ClientCertificateReference';
import { thumbPrintValidation } from '../common/thumbprintInput/validator'
import { ItemType } from './ItemType';

export class ClientCertificateReferenceForm {
    public static getForm(formBuilder: FormWrapper, current: ClientCertificateReference | undefined, path: string, runtime: ItemType[]): FormWrapper {
        let form = formBuilder
            .group({
                [ClientCertificateReference.FIELD_clientCertificateFindType]: [formBuilder.createFieldValue(current, ClientCertificateReference.FIELD_clientCertificateFindType, path, null, runtime), Validators.required],
                [ClientCertificateReference.FIELD_clientCertificateFindValue]: [formBuilder.createFieldValue(current, ClientCertificateReference.FIELD_clientCertificateFindValue, path, null, runtime), [Validators.required, thumbPrintValidation]]
            })
            .onChange<number>(ClientCertificateReference.FIELD_clientCertificateFindType, (selected, wrapper) => {
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
                    wrapper.form.get(ClientCertificateReference.FIELD_clientCertificateFindType)!.setValue(runtime[`${path}.${ClientCertificateReference.FIELD_clientCertificateFindType}`]);
                }
            })
            .triggerHandler(ClientCertificateReference.FIELD_clientCertificateFindValue, current && current.clientCertificateFindValue);
        return form;
    }
}
