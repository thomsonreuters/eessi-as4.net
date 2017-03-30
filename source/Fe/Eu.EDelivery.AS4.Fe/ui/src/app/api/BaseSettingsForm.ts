import { BaseSettings } from './BaseSettings';
import { FormBuilder, FormGroup } from '@angular/forms';
import { CertificateStoreForm } from './CertificateStoreForm';

export class BaseSettingsForm {
    public static getForm(formBuilder: FormBuilder, current: BaseSettings): FormGroup {
        return formBuilder.group({
            idFormat: [current && current.idFormat],
            certificateStore: CertificateStoreForm.getForm(formBuilder, current && current.certificateStore),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: BaseSettings) {
        form.get(BaseSettings.FIELD_idFormat).reset({ value: current && current.idFormat });
        CertificateStoreForm.patchForm(formBuilder, <FormGroup>form.get(BaseSettings.FIELD_certificateStore), current && current.certificateStore);
    }
}