import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { KeyEncryption } from './KeyEncryption';
import { FormWrapper } from './../common/form.service';

export class KeyEncryptionForm {
    public static defaultTransportAlgorithm: string = 'http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p';
    public static defaultDigestAlgorithm: string = 'http://www.w3.org/2000/09/xmldsig#sha1';
    public static transportAlgorithm: string = 'http://www.w3.org/2009/xmlenc11#rsa-oaep';
    public static defaultMgfAlgorithm: string = 'http://www.w3.org/2009/xmlenc11#mgf1sha1';

    public static getForm(formBuilder: FormWrapper, current: KeyEncryption): FormWrapper {
        let form = formBuilder
            .group({
                [KeyEncryption.FIELD_transportAlgorithm]: [current && current.transportAlgorithm, Validators.required],
                [KeyEncryption.FIELD_digestAlgorithm]: [current && current.digestAlgorithm, Validators.required],
                [KeyEncryption.FIELD_mgfAlgorithm]: [current && current.mgfAlgorithm, Validators.required]
            })
            .onChange<string>(KeyEncryption.FIELD_transportAlgorithm, (value, wrapper) => {
                let mgf = wrapper.form.get(KeyEncryption.FIELD_mgfAlgorithm);
                if (value !== KeyEncryptionForm.transportAlgorithm) {
                    mgf.disable();
                } else {
                    mgf.enable();
                }
            });
        return form;
    }
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: KeyEncryption, isDisabled: boolean) {
        // form.get(KeyEncryption.FIELD_transportAlgorithm).reset({ value: (current && current.transportAlgorithm) || KeyEncryptionForm.defaultTransportAlgorithm, disabled: !!!current || isDisabled });
        // form.get(KeyEncryption.FIELD_digestAlgorithm).reset({ value: (current && current.digestAlgorithm) || KeyEncryptionForm.defaultDigestAlgorithm, disabled: !!!current || isDisabled });
        // form.get(KeyEncryption.FIELD_mgfAlgorithm).reset({ value: (current && current.mgfAlgorithm) || KeyEncryptionForm.defaultMgfAlgorithm, disabled: !!!current || isDisabled || current.transportAlgorithm !== this.transportAlgorithm });
    }
}
