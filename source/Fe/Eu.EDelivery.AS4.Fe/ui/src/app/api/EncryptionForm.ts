import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import 'rxjs/add/operator/distinctuntilchanged';

import { ItemType } from './ItemType';
import { Encryption, PublicKeyFindCriteria, PublicKeyCertificate } from './Encryption';
import { thumbPrintValidation } from '../validators/thumbprintValidator';
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
                [Encryption.FIELD_publicKeyType]: [2],
                [Encryption.FIELD_publicKeyInformation]: formBuilder.formBuilder.group({
                    publicKeyFindType: [formBuilder.createFieldValue(current, Encryption.FIELD_isEnabled, `${path}.${Encryption.FIELD_publicKeyInformation}.publicKeyFindType`, 0, runtime)],
                    publicKeyFindValue: [formBuilder.createFieldValue(current, Encryption.FIELD_isEnabled, `${path}.${Encryption.FIELD_publicKeyInformation}.publicKeyFindValue`, null, runtime)]
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
            .onChange<number>('publicKeyInformation', (result, wrapper) => {
                const typeControl = wrapper.form!.get('publicKeyInformation.publicKeyFindType')!;
                const valueControl = wrapper.form!.get('publicKeyInformation.publicKeyFindValue')!;

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
            .triggerHandler('publicKeyInformation', current && current.publicKeyInformation)
            .triggerHandler(Encryption.FIELD_isEnabled, current && current.isEnabled);
        return form;
    }
}
