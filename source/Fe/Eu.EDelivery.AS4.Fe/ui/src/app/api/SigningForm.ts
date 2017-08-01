import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ItemType } from './ItemType';
import { FormWrapper } from './../common/form.service';
import { thumbPrintValidation } from '../validators/thumbprintValidator';
import { Signing } from './Signing';

export class SigningForm {
    public static getForm(formBuilder: FormWrapper, current: Signing, path: string, runtime: ItemType[]): FormWrapper {
        let form = formBuilder
            .group({
                [Signing.FIELD_isEnabled]: [formBuilder.createFieldValue(current, Signing.FIELD_isEnabled, path, false, runtime)],
                [Signing.FIELD_privateKeyFindType]: [formBuilder.createFieldValue(current, Signing.FIELD_privateKeyFindType, path, 0, runtime), Validators.required],
                [Signing.FIELD_privateKeyFindValue]: [formBuilder.createFieldValue(current, Signing.FIELD_privateKeyFindValue, path, 0, runtime), Validators.required],
                [Signing.FIELD_keyReferenceMethod]: [formBuilder.createFieldValue(current, Signing.FIELD_keyReferenceMethod, path, 0, runtime), Validators.required],
                [Signing.FIELD_algorithm]: [formBuilder.createFieldValue(current, Signing.FIELD_algorithm, path, 0, runtime), Validators.required],
                [Signing.FIELD_hashFunction]: [formBuilder.createFieldValue(current, Signing.FIELD_hashFunction, path, 0, runtime), Validators.required]
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
                if (+result === 0) {
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
}
