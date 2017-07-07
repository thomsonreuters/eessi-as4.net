import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { thumbPrintValidation } from '../validators/thumbprintValidator';
import { Signing } from './Signing';

export class SigningForm {
    public static getForm(formBuilder: FormWrapper, current: Signing): FormWrapper {
        let form = formBuilder
            .group({
                [Signing.FIELD_isEnabled]: [!!(current && current.isEnabled)],
                [Signing.FIELD_privateKeyFindValue]: [current && current.privateKeyFindValue, Validators.required],
                [Signing.FIELD_privateKeyFindType]: [current && current.privateKeyFindType, Validators.required],
                [Signing.FIELD_keyReferenceMethod]: [current && current.keyReferenceMethod, Validators.required],
                [Signing.FIELD_algorithm]: [(current == null || current.algorithm == null) ? 'http://www.w3.org/2001/04/xmldsig-more#rsa-sha256' : current.algorithm, Validators.required],
                [Signing.FIELD_hashFunction]: [(current == null || current.hashFunction == null) ? 'http://www.w3.org/2001/04/xmlenc#sha256' : current.hashFunction, Validators.required],
            })
            .onChange(Signing.FIELD_isEnabled, (value, wrapper) => {
                if (value) {
                    wrapper.enable([Signing.FIELD_isEnabled]);
                } else {
                    wrapper.disable([Signing.FIELD_isEnabled]);
                }
            })
            .onChange<number>(Signing.FIELD_privateKeyFindType, (result, wrapper) => {
                const value = wrapper.form!.get(Signing.FIELD_privateKeyFindValue)!;

                value.clearValidators();
                if (result === 0) {
                    value.setValidators([Validators.required, thumbPrintValidation]);
                } else {
                    value.setValidators(Validators.required);
                }
                value.updateValueAndValidity();
            })
            .triggerHandler(Signing.FIELD_privateKeyFindType, current && current.privateKeyFindType)
            .triggerHandler(Signing.FIELD_isEnabled, current && current.isEnabled);
        return form;
    }
    private static defaultHashFunction: string = 'http://www.w3.org/2001/04/xmlenc#sha256';
    private static defaultAlgorithm: string = 'http://www.w3.org/2001/04/xmldsig-more#rsa-sha256';
    private static defaultKeyReferenceMethod: number = 0;
}
