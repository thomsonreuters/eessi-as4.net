import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ItemType } from './ItemType';
import { FormWrapper } from './../common/form.service';
import { thumbPrintValidation } from '../common/thumbprintInput/validator'
import { Signing } from './Signing';

export class SigningForm {
    public static getForm(formBuilder: FormWrapper, current: Signing, path: string, runtime: ItemType[]): FormWrapper {
        let form = formBuilder
            .group({
                [Signing.FIELD_isEnabled]: [formBuilder.createFieldValue(current, Signing.FIELD_isEnabled, path, false, runtime)],
                [Signing.FIELD_signingCertificateInformation]: formBuilder.formBuilder.group({
                    certificateFindType: [formBuilder.createFieldValue(current, Signing.FIELD_signingCertificateInformation + '.certificateFindType', path, 0, runtime)],
                    certificateFindValue: [formBuilder.createFieldValue(current, Signing.FIELD_signingCertificateInformation + '.certificateFindValue', path, 0, runtime)]
                }),
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
            .triggerHandler(Signing.FIELD_isEnabled, current && current.isEnabled);
        form.form
            .get(Signing.FIELD_signingCertificateInformation + '.certificateFindType')!
            .valueChanges
            .distinctUntilChanged()
            .subscribe((result) => {
                const findType = formBuilder.form!.get(Signing.FIELD_signingCertificateInformation + '.certificateFindType')!;
                const findValue = formBuilder.form!.get(Signing.FIELD_signingCertificateInformation + '.certificateFindValue')!;
                if (!!!findType) {
                    return;
                }
                findValue.clearValidators();
                if (+result === 0) {
                    findValue.setValidators([Validators.required, thumbPrintValidation]);
                } else {
                    findValue.setValidators(Validators.required);
                }
                findValue.updateValueAndValidity();
            });
        return form;
    }
}
