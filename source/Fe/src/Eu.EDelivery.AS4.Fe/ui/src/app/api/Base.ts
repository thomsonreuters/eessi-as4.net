import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { CertificateStore } from './CertificateStore';

export class Base {
    idFormat: string;
    certificateStore: CertificateStore;

    static FIELD_idFormat: string = 'idFormat';
    static FIELD_certificateStore: string = 'certificateStore';

    static getForm(formBuilder: FormBuilder, current: Base): FormGroup {
        return formBuilder.group({
            [this.FIELD_idFormat]: [current && current.idFormat, Validators.required],
            [this.FIELD_certificateStore]: CertificateStore.getForm(formBuilder, current && current.certificateStore)
        });
    }
    static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Base) {
        form.get(this.FIELD_idFormat).reset({ value: current && current.idFormat });
        CertificateStore.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_certificateStore), current && current.certificateStore);
    }
}
