import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { KeyEncryption } from './KeyEncryption';
import { FormWrapper } from './../common/form.service';

export class KeyEncryptionForm {
    public static defaultTransportAlgorithm: string = 'http://www.w3.org/2009/xmlenc11#rsa-oaep';
    public static defaultDigestAlgorithm: string = 'http://www.w3.org/2000/09/xmldsig#sha1';
    public static transportAlgorithm: string = 'http://www.w3.org/2009/xmlenc11#rsa-oaep';
    public static defaultMgfAlgorithm: string = 'http://www.w3.org/2009/xmlenc11#mgf1sha1';

    public static getForm(formBuilder: FormWrapper, current: KeyEncryption): FormWrapper {
        let form = formBuilder
            .group({
                [KeyEncryption.FIELD_transportAlgorithm]: [!!!current ? null : !!!current.transportAlgorithm ? this.defaultTransportAlgorithm : current.transportAlgorithm, Validators.required],
                [KeyEncryption.FIELD_digestAlgorithm]: [!!!current ? null : !!!current.digestAlgorithm ? this.defaultDigestAlgorithm : current.digestAlgorithm, Validators.required],
                [KeyEncryption.FIELD_mgfAlgorithm]: [!!!current ? null : !!!current.mgfAlgorithm ? this.defaultMgfAlgorithm : current.mgfAlgorithm, Validators.required]
            })
            .onChange<string>(KeyEncryption.FIELD_transportAlgorithm, (value, wrapper) => {
                let mgf = wrapper.form.get(KeyEncryption.FIELD_mgfAlgorithm)!;
                if (value !== KeyEncryptionForm.transportAlgorithm) {
                    mgf.disable();
                } else {
                    mgf.enable();
                }
            });
        return form;
    }
}
