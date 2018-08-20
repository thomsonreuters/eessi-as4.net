import { CertificateStore } from './CertificateStore';
import { FormBuilder, FormGroup } from '@angular/forms';
import { RepositoryForm } from './RepositoryForm';

export class CertificateStoreForm {
    public static getForm(formBuilder: FormBuilder, current: CertificateStore): FormGroup {
        return formBuilder.group({
            storeName: [current && current.storeName],
            repository: RepositoryForm.getForm(formBuilder, current && current.repository),
        });
    }
}
