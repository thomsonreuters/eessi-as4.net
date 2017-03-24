/* tslint: disable */
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

export class KeyEncryption {
    transportAlgorithm: string;
    digestAlgorithm: string;
    mgfAlgorithm: string;    

    static FIELD_transportAlgorithm: string = 'transportAlgorithm';
    static FIELD_digestAlgorithm: string = 'digestAlgorithm';
    static FIELD_mgfAlgorithm: string = 'mgfAlgorithm';  
}
