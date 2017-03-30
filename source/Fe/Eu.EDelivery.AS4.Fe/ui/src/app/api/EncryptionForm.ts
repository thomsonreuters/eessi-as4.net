import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Encryption } from './Encryption';
import { thumbPrintValidation } from '../validators/thumbprintValidator';
import { KeyEncryptionForm } from './KeyEncryptionForm';

export class EncryptionForm {
    public static getForm(formBuilder: FormBuilder, current: Encryption): FormGroup {
        let form = formBuilder.group({
            isEnabled: [!!(current && current.isEnabled), Validators.required],
            algorithm: [current && current.algorithm, Validators.required],
            publicKeyFindType: [current && current.publicKeyFindType, Validators.required],
            publicKeyFindValue: [current && current.publicKeyFindValue, Validators.required],
            keyTransport: KeyEncryptionForm.getForm(formBuilder, current && current.keyTransport),
        });
        this.setupForm(form);
        return form;
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Encryption) {
        form.get(Encryption.FIELD_isEnabled).reset({ value: current && current.isEnabled, disabled: !!!current });
        form.get(Encryption.FIELD_algorithm).reset({ value: (current && current.algorithm) || Encryption.defaultAlgorithm, disabled: !!!current || !current.isEnabled });
        form.get(Encryption.FIELD_publicKeyFindType).reset({ value: current && current.publicKeyFindType, disabled: !!!current || !current.isEnabled });
        form.get(Encryption.FIELD_publicKeyFindValue).reset({ value: current && current.publicKeyFindValue, disabled: !!!current || !current.isEnabled });
        KeyEncryptionForm.patchForm(formBuilder, <FormGroup>form.get(Encryption.FIELD_keyTransport), current && current.keyTransport, !!!current || !current.isEnabled);
    }
    private static setupForm(form: FormGroup) {
        let fields = [Encryption.FIELD_algorithm, Encryption.FIELD_keyTransport, Encryption.FIELD_publicKeyFindType, Encryption.FIELD_publicKeyFindValue];
        let value = form.get(Encryption.FIELD_publicKeyFindValue);
        let isEnabled = form.get(Encryption.FIELD_isEnabled);

        form.get(Encryption.FIELD_publicKeyFindType)
            .valueChanges
            .subscribe((result: number) => {
                value.clearValidators();
                if (+result === 0) {
                    value.setValidators([Validators.required, thumbPrintValidation]);
                } else {
                    value.setValidators(Validators.required);
                }
                value.updateValueAndValidity();
            });

        isEnabled
            .valueChanges
            .map((result) => !!result)
            .subscribe((result) => {
                if (result) {
                    fields.forEach((el) => form.get(el).enable());
                    form.get(Encryption.FIELD_keyTransport).enable();
                } else {
                    fields.forEach((el) => form.get(el).disable());
                    form.get(Encryption.FIELD_keyTransport).disable();
                }
            });
    }
}