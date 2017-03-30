/* tslint:disable */
import { FormBuilder, FormGroup, FormArray, FormControl, AbstractControl } from '@angular/forms';

export class Decryption {
    encryption: number;
    privateKeyFindValue: string;
    privateKeyFindType: number;

    static FIELD_encryption: string = 'encryption';
    static FIELD_privateKeyFindValue: string = 'privateKeyFindValue';
    static FIELD_privateKeyFindType: string = 'privateKeyFindType';   
}
