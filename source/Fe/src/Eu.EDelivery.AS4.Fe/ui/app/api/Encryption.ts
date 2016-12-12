/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Encryption {
	isEnabled: boolean;
	algorithm: string;
	publicKeyFindType: number;
	publicKeyFindValue: string;
	keyTransport: string;

	static FIELD_isEnabled: string = 'isEnabled';	
	static FIELD_algorithm: string = 'algorithm';	
	static FIELD_publicKeyFindType: string = 'publicKeyFindType';	
	static FIELD_publicKeyFindValue: string = 'publicKeyFindValue';	
	static FIELD_keyTransport: string = 'keyTransport';	

	static getForm(formBuilder: FormBuilder, current: Encryption): FormGroup {
		return formBuilder.group({
			isEnabled: [!!(current && current.isEnabled)],
			algorithm: [current && current.algorithm],
			publicKeyFindType: [current && current.publicKeyFindType],
			publicKeyFindValue: [current && current.publicKeyFindValue],
			keyTransport: [current && current.keyTransport],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Encryption) {
		form.removeControl('isEnabled');
		form.addControl('isEnabled', formBuilder.control(current && current.isEnabled));
		form.removeControl('algorithm');
		form.addControl('algorithm', formBuilder.control(current && current.algorithm));
		form.removeControl('publicKeyFindType');
		form.addControl('publicKeyFindType', formBuilder.control(current && current.publicKeyFindType));
		form.removeControl('publicKeyFindValue');
		form.addControl('publicKeyFindValue', formBuilder.control(current && current.publicKeyFindValue));
		form.removeControl('keyTransport');
		form.addControl('keyTransport', formBuilder.control(current && current.keyTransport));

	}
}
