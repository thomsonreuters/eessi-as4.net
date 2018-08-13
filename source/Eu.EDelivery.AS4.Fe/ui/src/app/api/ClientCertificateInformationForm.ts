import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { ClientCertificateInformation } from './ClientCertificateInformation';
import { thumbPrintValidation } from '../common/thumbprintInput/validator'
import { ItemType } from './ItemType';

export class ClientCertificateInformationForm {
    public static getForm(formBuilder: FormWrapper, current: ClientCertificateInformation | undefined, path: string, runtime: ItemType[]): FormWrapper {
        let form = formBuilder
            .group({
                [ClientCertificateInformation.FIELD_clientCertificateFindType]: [formBuilder.createFieldValue(current, ClientCertificateInformation.FIELD_clientCertificateFindType, path, null, runtime), Validators.required],
                [ClientCertificateInformation.FIELD_clientCertificateFindValue]: [formBuilder.createFieldValue(current, ClientCertificateInformation.FIELD_clientCertificateFindValue, path, null, runtime), [Validators.required, thumbPrintValidation]]
            })
            .onChange<number>(ClientCertificateInformation.FIELD_clientCertificateFindType, (selected, wrapper) => {
                let findValue = wrapper.form.get(ClientCertificateInformation.FIELD_clientCertificateFindValue)!;
                findValue.clearValidators();
                if (+selected === 0) {
                    findValue.setValidators([Validators.required, thumbPrintValidation]);
                } else {
                    findValue.setValidators(Validators.required);
                }
            })
            .onStatusChange(undefined, (status, wrapper) => {
                if (status !== 'DISABLED' && !!!wrapper.form.get(ClientCertificateInformation.FIELD_clientCertificateFindType)!.value) {
                    wrapper.form.get(ClientCertificateInformation.FIELD_clientCertificateFindType)!.setValue(runtime[`${path}.${ClientCertificateInformation.FIELD_clientCertificateFindType}`]);
                }
            })
            .triggerHandler(ClientCertificateInformation.FIELD_clientCertificateFindValue, current && current.clientCertificateFindValue);
        return form;
    }
}
