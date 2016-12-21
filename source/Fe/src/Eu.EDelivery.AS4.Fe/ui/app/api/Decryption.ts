/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Decryption {
    encryption: number;
    privateKeyFindValue: string;
    privateKeyFindType: number;

    static FIELD_encryption: string = 'encryption';
    static FIELD_privateKeyFindValue: string = 'privateKeyFindValue';
    static FIELD_privateKeyFindType: string = 'privateKeyFindType';

    static getForm(formBuilder: FormBuilder, current: Decryption): FormGroup {
        let form = formBuilder.group({
            encryption: [(current == null || current.encryption == null) ? 0 : current.encryption],
            privateKeyFindValue: [current && current.privateKeyFindValue, this.validateKey],
            privateKeyFindType: [(current == null || current.privateKeyFindType == null) ? 0 : current.privateKeyFindType, this.validateKeyType],
        });
        Decryption.setupForm(form);
        return form;
    }
    /// Patch up all the formArray controls
    static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Decryption) {
        form.removeControl('encryption');
        form.addControl('encryption', formBuilder.control(current && current.encryption));
        form.removeControl('privateKeyFindValue');
        form.addControl('privateKeyFindValue', formBuilder.control(current && current.privateKeyFindValue));
        form.removeControl('privateKeyFindType');
        form.addControl('privateKeyFindType', formBuilder.control(current && current.privateKeyFindType));
        Decryption.setupForm(form);
    }

    private static setupForm(formGroup: FormGroup) {
        formGroup.get(Decryption.FIELD_encryption).valueChanges.subscribe(result => {
            formGroup.get(Decryption.FIELD_privateKeyFindValue).updateValueAndValidity();
            formGroup.get(Decryption.FIELD_privateKeyFindType).updateValueAndValidity();
        });
    }

    private static validateKey(control: FormGroup) {
        let encryptionValue = control && control.parent && Decryption.getEncryptionValue(<FormGroup>control.parent);
        if ((encryptionValue === 2) && !!!control.value) {
            return {
                required: true
            }
        }

        return null;
    }

    private static validateKeyType(control: FormGroup) {
        let encryptionValue = control && control.parent && Decryption.getEncryptionValue(<FormGroup>control.parent);
        if ((encryptionValue === 2) && control.value < 0) {
            return {
                required: true
            }
        }

        return null;
    }

    private static getEncryptionValue(control: FormGroup): number {
        return +control.get(Decryption.FIELD_encryption).value;
    }
}
