/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

import { Repository } from './Repository';

export class CertificateStore {
    storeName: string;
    repository: Repository;

    static FIELD_storeName: string = 'storeName';
    static FIELD_repository: string = 'repository';

    static getForm(formBuilder: FormBuilder, current: CertificateStore): FormGroup {
        return formBuilder.group({
            storeName: [current && current.storeName],
            repository: Repository.getForm(formBuilder, current && current.repository)
        });
    }
}
