import { ItemType } from './ItemType';

import { FormWrapper } from './../common/form.service';
import { thumbPrintValidation } from '../validators/thumbprintValidator';
import { Decryption } from './Decryption';

export class DecryptionForm {
    public static getForm(formBuilder: FormWrapper, current: Decryption, path: string, runtime: ItemType[]): FormWrapper {
        let form = formBuilder
            .group({
                encryption: [formBuilder.createFieldValue(current, Decryption.FIELD_encryption, path, 0, runtime)],
                privateKeyFindType: [formBuilder.createFieldValue(current, Decryption.FIELD_privateKeyFindType, path, 0, runtime)],
                privateKeyFindValue: [formBuilder.createFieldValue(current, Decryption.FIELD_privateKeyFindValue, path, null, runtime)]
            })
            .onChange(Decryption.FIELD_encryption, (result, wrapper) => {
                if (wrapper.form.disabled) {
                    // Force disable the form
                    wrapper.disable([Decryption.FIELD_encryption]);
                    return;
                }

                if (+result === 2 || +result === 0) {
                    wrapper.enable([Decryption.FIELD_encryption]);
                } else {
                    wrapper.disable([Decryption.FIELD_encryption]);
                }
            })
            .onChange(Decryption.FIELD_privateKeyFindType, (result, wrapper) => {
                wrapper.form.get(Decryption.FIELD_privateKeyFindValue)!.updateValueAndValidity();
            })
            .triggerHandler(Decryption.FIELD_encryption, current && current.encryption)
            .triggerHandler(Decryption.FIELD_privateKeyFindType, current && current.privateKeyFindType);
        return form;
    }
}
