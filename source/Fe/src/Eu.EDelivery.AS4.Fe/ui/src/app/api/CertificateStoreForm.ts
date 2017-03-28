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
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: CertificateStore) {
        form.get(CertificateStore.FIELD_storeName).reset({ value: current && current.storeName });
        RepositoryForm.patchForm(formBuilder, <FormGroup>form.get(CertificateStore.FIELD_repository), current && current.repository);
    }
}
