import { Validators } from '@angular/forms';
import { ItemType } from './ItemType';

import { FormWrapper } from './../common/form.service';
import { thumbPrintValidation } from '../common/thumbprintInput/validator'
import { Decryption } from './Decryption';

export class DecryptionForm {
    public static getForm(formBuilder: FormWrapper, current: Decryption, path: string, runtime: ItemType[]): FormWrapper {
        let form = formBuilder
            .group({
                encryption: [formBuilder.createFieldValue(current, Decryption.FIELD_encryption, path, 0, runtime)],
                [Decryption.FIELD_decryptCertificateInformation]: formBuilder.formBuilder.group({
                    certificateFindType: [formBuilder.createFieldValue(current, Decryption.FIELD_decryptCertificateInformation + '.certificateFindType', path, 0, runtime)],
                    certificateFindValue: [formBuilder.createFieldValue(current, Decryption.FIELD_decryptCertificateInformation + '.certificateFindValue', path, null, runtime)]
                })
            })
            .onChange<number>(Decryption.FIELD_decryptCertificateInformation, (result, wrapper) => {
                const value = wrapper.form!.get(Decryption.FIELD_decryptCertificateInformation + '.certificateFindType')!;

                value.clearValidators();
                if (+result === 0) {
                    value.setValidators([Validators.required, thumbPrintValidation]);
                } else {
                    value.setValidators(Validators.required);
                }
                value.updateValueAndValidity({ emitEvent: false });
            });
        return form;
    }
}
