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
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Decryption) {
		form.removeControl('encryption');
		form.addControl('encryption', formBuilder.control(current && current.encryption));
		form.removeControl('privateKeyFindValue');
		form.addControl('privateKeyFindValue', formBuilder.control(current && current.privateKeyFindValue));
		form.removeControl('privateKeyFindType');
		form.addControl('privateKeyFindType', formBuilder.control(current && current.privateKeyFindType));

	}
}
