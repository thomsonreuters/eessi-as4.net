/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Decryption {
	encryption: number;
	privateKeyFindValue: string;
	privateKeyFindType: number;

	static FIELD_encryption: string = 'encryption';	
	static FIELD_privateKeyFindValue: string = 'privateKeyFindValue';	
	static FIELD_privateKeyFindType: string = 'privateKeyFindType';	

	static getForm(formBuilder: FormBuilder, current: Decryption): FormGroup {
		return formBuilder.group({
			encryption: [current && current.encryption],
			privateKeyFindValue: [current && current.privateKeyFindValue],
			privateKeyFindType: [current && current.privateKeyFindType],
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: Decryption) {
	}
}
