/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Signing } from "./Signing";
import { Encryption } from "./Encryption";

export class Security {
	signing: Signing;
	encryption: Encryption;

	static FIELD_signing: string = 'signing';
	static FIELD_encryption: string = 'encryption';

	static getForm(formBuilder: FormBuilder, current: Security): FormGroup {
		return formBuilder.group({
			signing: Signing.getForm(formBuilder, current && current.signing),
			encryption: Encryption.getForm(formBuilder, current && current.encryption),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Security) {

		form.removeControl('signing');
		form.addControl('signing', Signing.getForm(formBuilder, current && current.signing));
		form.removeControl('encryption');
		form.addControl('encryption', Encryption.getForm(formBuilder, current && current.encryption));
	}
}
