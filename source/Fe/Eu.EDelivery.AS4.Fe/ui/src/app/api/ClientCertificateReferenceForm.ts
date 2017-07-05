import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ClientCertificateReference } from './ClientCertificateReference';
import { thumbPrintValidation } from '../validators/thumbprintValidator';

export class ClientCertificateReferenceForm {
    public static getForm(formBuilder: FormBuilder, current: ClientCertificateReference): FormGroup {
        let form = formBuilder.group({
            clientCertificateFindType: [current && current.clientCertificateFindType, Validators.required],
            clientCertificateFindValue: [current && current.clientCertificateFindValue, [Validators.required, thumbPrintValidation]]
        });
        this.setupForm(form);
        return form;
    }
    static setupForm(form: FormGroup) {
        let findValue = form.get(ClientCertificateReference.FIELD_clientCertificateFindValue);
        form.get(ClientCertificateReference.FIELD_clientCertificateFindType)
            .valueChanges
            .subscribe((result: number) => {
                findValue.clearValidators();
                if (+result === 0)
                    findValue.setValidators([Validators.required, thumbPrintValidation]);
                else
                    findValue.setValidators(Validators.required);
            });
    }
}