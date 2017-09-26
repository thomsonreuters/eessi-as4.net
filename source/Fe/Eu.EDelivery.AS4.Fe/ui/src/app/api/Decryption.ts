/* tslint:disable */
import { FormBuilder, FormGroup, FormArray, FormControl, AbstractControl } from '@angular/forms';

export class Decryption {
    encryption: number;
    decryptCertificateInformation: {
        certificateFindType: string,
        certificateFindValue: string
    };

    static FIELD_encryption: string = 'encryption';
    static FIELD_decryptCertificateInformation: string = 'decryptCertificateInformation';
}
