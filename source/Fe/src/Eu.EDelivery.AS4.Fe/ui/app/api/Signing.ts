/* tslint:disable */
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

	static getForm(formBuilder: FormBuilder, current: Signing): FormGroup {
		return formBuilder.group({
			isEnabled: [!!(current && current.isEnabled)],
			privateKeyFindValue: [current && current.privateKeyFindValue],
			privateKeyFindType: [current && current.privateKeyFindType],
			keyReferenceMethod: [current && current.keyReferenceMethod],
			algorithm: [current && current.algorithm],
			hashFunction: [current && current.hashFunction],
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: Signing) {
	}
}
