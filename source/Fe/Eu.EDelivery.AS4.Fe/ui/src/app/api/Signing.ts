/* tslint:disable */
import { Validators } from '@angular/forms';
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Signing {
	isEnabled: boolean;
	privateKeyFindValue: string;
	privateKeyFindType: number;
	keyReferenceMethod: number;
	algorithm: string;
	hashFunction: string;

	static FIELD_isEnabled: string = 'isEnabled';
	static FIELD_privateKeyFindValue: string = 'privateKeyFindValue';
	static FIELD_privateKeyFindType: string = 'privateKeyFindType';
	static FIELD_keyReferenceMethod: string = 'keyReferenceMethod';
	static FIELD_algorithm: string = 'algorithm';
	static FIELD_hashFunction: string = 'hashFunction';	
}
