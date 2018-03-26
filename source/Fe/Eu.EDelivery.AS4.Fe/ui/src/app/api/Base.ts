import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { CertificateStore } from './CertificateStore';

export class Base {
    public static FIELD_idFormat: string = 'idFormat';
    public static FIELD_retentionPeriod: string = "retentionPeriod";
    public static FIELD_certificateStore: string = 'certificateStore';

    public idFormat: string;
    public retentionPeriod: number;
    public certificateStore: CertificateStore;
}
