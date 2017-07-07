import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { thumbPrintValidation } from '../validators/thumbprintValidator';
import { Decryption } from './Decryption';

export class DecryptionForm {
    public static getForm(formBuilder: FormWrapper, current: Decryption): FormWrapper {
        let form = formBuilder
            .group({
                encryption: [(current == null || current.encryption == null) ? 0 : current.encryption],
                privateKeyFindValue: [current && current.privateKeyFindValue, this.validateKey],
                privateKeyFindType: [current && current.privateKeyFindType, this.validateKeyType],
            })
            .onChange(Decryption.FIELD_encryption, (result, wrapper) => {
                if (+result === 2 || result === 0) {
                    wrapper.disable([Decryption.FIELD_encryption]);
                } else {
                    wrapper.enable([Decryption.FIELD_encryption]);
                }
            })
            .triggerHandler(Decryption.FIELD_encryption, current && current.encryption);
        return form;
    }
    private static validateKey(control: FormGroup): any {
        let encryptionValue = control && control.parent && DecryptionForm.getEncryptionValue(<FormGroup>control.parent);
        let findValue = control && control.parent && control.parent.get(Decryption.FIELD_privateKeyFindValue);
        let findType = control && control.parent && control.parent.get(Decryption.FIELD_privateKeyFindType);
        if (encryptionValue === 2) {
            if (!!!control.value) {
                return {
                    required: true
                }
            }
        }

        if (findType && findType.enabled && +findType.value === 0) {
            return thumbPrintValidation(<FormControl>findValue, encryptionValue === 2)
        }

        return null;
    }
    private static validateKeyType(control: FormGroup) {
        let encryptionValue = control && control.parent && DecryptionForm.getEncryptionValue(<FormGroup>control.parent);
        if ((encryptionValue === 2) && control.value < 0) {
            return {
                required: true
            }
        }

        return null;
    }

    private static getEncryptionValue(control: FormGroup): number {
        return +control.get(Decryption.FIELD_encryption)!.value;
    }
}