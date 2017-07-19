import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import 'rxjs/add/operator/distinctuntilchanged';

import { Encryption, PublicKeyFindCriteria, PublicKeyCertificate } from './Encryption';
import { thumbPrintValidation } from '../validators/thumbprintValidator';
import { KeyEncryptionForm } from './KeyEncryptionForm';
import { FormWrapper } from './../common/form.service';

export class EncryptionForm {

    public static getForm(formBuilder: FormWrapper, current: Encryption): FormWrapper {
        let previousFindType = null;
        let form = formBuilder
            .group({
                isEnabled: [!!(current && current.isEnabled), Validators.required],
                algorithm: [!!!current ? null : !!!current.algorithm ? this.DefaultAlgorithm : current.algorithm, Validators.required],
                algorithmKeySize: [!!!current ? null : !!!current.algorithmKeySize ? this.DefaultAlgorithmKeySize : current.algorithmKeySize, Validators.required],
                publicKeyType: [2],
                publicKeyInformation: formBuilder.formBuilder.group({
                    publicKeyFindType: [!!!current || !!!current.publicKeyInformation || !!!current.publicKeyInformation.publicKeyFindType ? 0 : current.publicKeyInformation.publicKeyFindType],
                    publicKeyFindValue:[!!!current || !!!current.publicKeyInformation || !!!current.publicKeyInformation.publicKeyFindValue ? null : current.publicKeyInformation.publicKeyFindValue]
                }),
                keyTransport: KeyEncryptionForm.getForm(formBuilder.subForm('keyTransport'), current && current.keyTransport).form,
            })
            .onChange(Encryption.FIELD_isEnabled, (result, wrapper) => {
                if (result) {
                    wrapper.enable([Encryption.FIELD_isEnabled]);
                } else {
                    wrapper.disable([Encryption.FIELD_isEnabled]);
                }
            })
            .onChange<number>('publicKeyInformation', (result, wrapper) => {
                const typeControl = wrapper.form!.get('publicKeyInformation.publicKeyFindType')!;
                const valueControl = wrapper.form!.get('publicKeyInformation.publicKeyFindValue')!;

                if (typeControl.value === previousFindType) {
                    return;
                }
                previousFindType = typeControl.value;
                valueControl.clearValidators();
                if (+typeControl.value === 0) {
                    valueControl.setValidators([Validators.required, thumbPrintValidation]);
                } else {
                    valueControl.setValidators(Validators.required);
                }
                valueControl.updateValueAndValidity();
            })
            .triggerHandler('publicKeyInformation', current && current.publicKeyInformation)
            .triggerHandler(Encryption.FIELD_isEnabled, current && current.isEnabled);                 return form;
    }
    private static DefaultAlgorithm = 'http://www.w3.org/2009/xmlenc11#aes128-gcm';
    private static DefaultAlgorithmKeySize = 128;
}
