/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class SigningVerification {
	signature: number;

	static FIELD_signature: string = 'signature';	

	static getForm(formBuilder: FormBuilder, current: SigningVerification): FormGroup {
		return formBuilder.group({
			signature: [current && current.signature],
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: SigningVerification) {
	}
}
