import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { KeyEncryption } from './KeyEncryption';
import { ItemType } from './ItemType';
import { FormWrapper } from './../common/form.service';

export class KeyEncryptionForm {
    private static disabledAlgo = 'http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p';
    public static getForm(formBuilder: FormWrapper, current: KeyEncryption, path: string, runtime: ItemType[]): FormWrapper {
        let form = formBuilder
            .group({
                [KeyEncryption.FIELD_transportAlgorithm]: [formBuilder.createFieldValue(current, KeyEncryption.FIELD_transportAlgorithm, path, null, runtime), Validators.required],
                [KeyEncryption.FIELD_digestAlgorithm]: [formBuilder.createFieldValue(current, KeyEncryption.FIELD_digestAlgorithm, path, null, runtime), Validators.required],
                [KeyEncryption.FIELD_mgfAlgorithm]: [formBuilder.createFieldValue(current, KeyEncryption.FIELD_mgfAlgorithm, path, null, runtime)]
            })
            .onChange<string>(KeyEncryption.FIELD_transportAlgorithm, (value, wrapper) => {
                let mgf = wrapper.form.get(KeyEncryption.FIELD_mgfAlgorithm)!;
                if (value === this.disabledAlgo) {
                    mgf.disable();
                } else {
                    mgf.enable();
                }
            });
        return form;
    }
}
