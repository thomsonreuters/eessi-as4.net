import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import 'rxjs/add/operator/distinctuntilchanged';

import { ItemType } from './ItemType';
import { Encryption } from './Encryption';
import { thumbPrintValidation } from '../common/thumbprintInput/validator'
import { KeyEncryptionForm } from './KeyEncryptionForm';
import { FormWrapper } from './../common/form.service';

export class EncryptionForm {

    public static getForm(formBuilder: FormWrapper, current: Encryption, path: string, runtime: ItemType[]): FormWrapper {
        let previousFindType = null;
        let form = formBuilder
            .group({
                [Encryption.FIELD_isEnabled]: [formBuilder.createFieldValue(current, Encryption.FIELD_isEnabled, path, false, runtime), Validators.required],
                [Encryption.FIELD_algorithm]: [formBuilder.createFieldValue(current, Encryption.FIELD_algorithm, path, null, runtime), Validators.required],
                [Encryption.FIELD_algorithmKeySize]: [formBuilder.createFieldValue(current, Encryption.FIELD_algorithmKeySize, path, null, runtime), Validators.required],
                [Encryption.FIELD_encryptionCertificateInformation]: formBuilder.formBuilder.group({
                    certificateFindType: [formBuilder.createFieldValue(current, Encryption.FIELD_encryptionCertificateInformation + '.certificateFindType', `${Encryption.FIELD_encryptionCertificateInformation}.certificateFindType`, 0, runtime)],
                    certificateFindValue: [formBuilder.createFieldValue(current, Encryption.FIELD_encryptionCertificateInformation + '.certificateFindValue', `${Encryption.FIELD_encryptionCertificateInformation}.certificateFindValue`, null, runtime)]
                }),
                [Encryption.FIELD_keyTransport]: KeyEncryptionForm.getForm(formBuilder.subForm('keyTransport'), current && current.keyTransport, `${path}.${Encryption.FIELD_keyTransport}`, runtime).form,
            })
            .onChange(Encryption.FIELD_isEnabled, (result, wrapper) => {
                if (result) {
                    wrapper.enable([Encryption.FIELD_isEnabled]);
                } else {
                    wrapper.disable([Encryption.FIELD_isEnabled]);
                }
            })
            .onChange<number>(Encryption.FIELD_encryptionCertificateInformation, (result, wrapper) => {
                const typeControl = wrapper.form!.get(`${Encryption.FIELD_encryptionCertificateInformation}.certificateFindType`)!;
                const valueControl = wrapper.form!.get(`${Encryption.FIELD_encryptionCertificateInformation}.certificateFindValue`)!;

                if (typeControl.value === previousFindType) {
                    return;
                }
                previousFindType = typeControl.value;
                valueControl.clearValidators();
                if (+typeControl.value === 0) {
                    valueControl.setValidators([Validators.required, thumbPrintValidation]);
                } else {
                    valueControl.setValidators(Validators.required);
                }
                valueControl.updateValueAndValidity();
            })
            .triggerHandler(Encryption.FIELD_encryptionCertificateInformation, current && current.encryptionCertificateInformation)
            .triggerHandler(Encryption.FIELD_isEnabled, current && current.isEnabled);
        return form;
    }
}
