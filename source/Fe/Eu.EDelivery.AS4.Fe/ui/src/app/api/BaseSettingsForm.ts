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
}
