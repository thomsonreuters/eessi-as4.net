import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Base } from './Base';
import { CertificateStoreForm } from './CertificateStoreForm';

export class BaseForm {
    public static getForm(formBuilder: FormBuilder, current: Base): FormGroup {
        return formBuilder.group({
            [Base.FIELD_idFormat]: [current && current.idFormat, Validators.required],
            [Base.FIELD_certificateStore]: CertificateStoreForm.getForm(formBuilder, current && current.certificateStore)
        });
    }
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Base) {
        form.get(Base.FIELD_idFormat).reset({ value: current && current.idFormat });
        CertificateStoreForm.patchForm(formBuilder, <FormGroup>form.get(Base.FIELD_certificateStore), current && current.certificateStore);
    }
}
