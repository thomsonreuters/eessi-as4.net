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
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Signing) {
		form.removeControl('isEnabled');
		form.addControl('isEnabled', formBuilder.control(current && current.isEnabled));
		form.removeControl('privateKeyFindValue');
		form.addControl('privateKeyFindValue', formBuilder.control(current && current.privateKeyFindValue));
		form.removeControl('privateKeyFindType');
		form.addControl('privateKeyFindType', formBuilder.control(current && current.privateKeyFindType));
		form.removeControl('keyReferenceMethod');
		form.addControl('keyReferenceMethod', formBuilder.control(current && current.keyReferenceMethod));
		form.removeControl('algorithm');
		form.addControl('algorithm', formBuilder.control(current && current.algorithm));
		form.removeControl('hashFunction');
		form.addControl('hashFunction', formBuilder.control(current && current.hashFunction));

	}
}
