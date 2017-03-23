import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { CertificateStore } from './CertificateStore';

export class Base {
    idFormat: string;
    certificateStore: CertificateStore;

    static FIELD_idFormat: string = 'idFormat';
    static FIELD_certificateStore: string = 'certificateStore';
}
