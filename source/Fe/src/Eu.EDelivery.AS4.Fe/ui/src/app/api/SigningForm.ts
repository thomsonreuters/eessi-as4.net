import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { thumbPrintValidation } from '../validators/thumbprintValidator';
import { Signing } from './Signing';

export class SigningForm {
    private static defaultHashFunction: string = 'http://www.w3.org/2001/04/xmlenc#sha256';
    private static defaultAlgorithm: string = 'http://www.w3.org/2001/04/xmldsig-more#rsa-sha256';
    private static defaultKeyReferenceMethod: number = 0;

    public static getForm(formBuilder: FormBuilder, current: Signing): FormGroup {
        let form = formBuilder.group({
            [Signing.FIELD_isEnabled]: [!!(current && current.isEnabled)],
            [Signing.FIELD_privateKeyFindValue]: [current && current.privateKeyFindValue, Validators.required],
            [Signing.FIELD_privateKeyFindType]: [current && current.privateKeyFindType, Validators.required],
            [Signing.FIELD_keyReferenceMethod]: [current && current.keyReferenceMethod, Validators.required],
            [Signing.FIELD_algorithm]: [(current == null || current.algorithm == null) ? 'http://www.w3.org/2001/04/xmldsig-more#rsa-sha256' : current.algorithm, Validators.required],
            [Signing.FIELD_hashFunction]: [(current == null || current.hashFunction == null) ? 'http://www.w3.org/2001/04/xmlenc#sha256' : current.hashFunction, Validators.required],
        });
        SigningForm.setupForm(form);
        return form;
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Signing) {
        form.get(Signing.FIELD_isEnabled).reset({ value: current && current.isEnabled, disabled: !!!current });
        form.get(Signing.FIELD_privateKeyFindValue).reset({ value: current && current.privateKeyFindValue, disabled: !!!current || !current.isEnabled });
        form.get(Signing.FIELD_privateKeyFindType).reset({ value: current && current.privateKeyFindType, disabled: !!!current || !current.isEnabled });
        form.get(Signing.FIELD_keyReferenceMethod).reset({ value: (current && current.keyReferenceMethod) || this.defaultKeyReferenceMethod, disabled: !!!current || !current.isEnabled });
        form.get(Signing.FIELD_algorithm).reset({ value: (current && current.algorithm) || this.defaultAlgorithm, disabled: !!!current || !current.isEnabled });
        form.get(Signing.FIELD_hashFunction).reset({ value: (current && current.hashFunction) || this.defaultHashFunction, disabled: !!!current || !current.isEnabled });
    }

    private static setupForm(form: FormGroup) {
        let fields = Object.keys(this).filter(key => key.startsWith('FIELD_') && !key.endsWith('isEnabled')).map(field => form.get(this[field]));
        let isEnabled = form.get(Signing.FIELD_isEnabled);
        let toggle = (value: boolean) => {
            if (value) {
                fields.forEach(field => field.enable());
            } else {
                fields.forEach(field => field.disable());
            }
        }
        toggle(isEnabled.value);
        isEnabled.valueChanges.subscribe((result) => toggle(result));
        let value = form.get(Signing.FIELD_privateKeyFindValue);
        form.get(Signing.FIELD_privateKeyFindType)
            .valueChanges
            .map((result) => +result)
            .subscribe((result) => {
                value.clearValidators();
                if (result === 0) {
                    value.setValidators([Validators.required, thumbPrintValidation]);
                } else {
                    value.setValidators(Validators.required);
                }
                value.updateValueAndValidity();
            });
    }
}
