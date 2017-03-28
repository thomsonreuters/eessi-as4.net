import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { thumbPrintValidation } from '../validators/thumbprintValidator';
import { Decryption } from './Decryption';

export class DecryptionForm {
    public static getForm(formBuilder: FormBuilder, current: Decryption): FormGroup {
        let form = formBuilder.group({
            encryption: [(current == null || current.encryption == null) ? 0 : current.encryption],
            privateKeyFindValue: [current && current.privateKeyFindValue, this.validateKey],
            privateKeyFindType: [current && current.privateKeyFindType, this.validateKeyType],
        });
        this.setupForm(form);
        return form;
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Decryption) {
        form.get(Decryption.FIELD_encryption).reset({ value: current && current.encryption, disabled: !!!current });
        form.get(Decryption.FIELD_privateKeyFindValue).reset({ value: current && current.privateKeyFindValue, disabled: !!!current });
        form.get(Decryption.FIELD_privateKeyFindType).reset({ value: current && current.privateKeyFindType, disabled: !!!current });
    }

    private static setupForm(form: FormGroup) {
        let fields = [Decryption.FIELD_privateKeyFindValue, Decryption.FIELD_privateKeyFindType];
        form
            .get(Decryption.FIELD_encryption)
            .valueChanges
            .map((result) => +result)
            .subscribe((result) => {
                fields.forEach((el) => {
                    let frm = form.get(el);
                    frm.updateValueAndValidity();
                    if (result === 2 || result === 0) {
                        frm.enable();
                    } else {
                        frm.disable();
                    }
                })
            });
        form
            .get(Decryption.FIELD_privateKeyFindType)
            .valueChanges
            .subscribe(() => {
                form.get(Decryption.FIELD_privateKeyFindValue).updateValueAndValidity();
            });
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
        return +control.get(Decryption.FIELD_encryption).value;
    }
}