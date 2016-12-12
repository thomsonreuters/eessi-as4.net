/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { SigningVerification } from "./SigningVerification";
import { Decryption } from "./Decryption";

export class ReceiveSecurity {
	signingVerification: SigningVerification;
	decryption: Decryption;

	static FIELD_signingVerification: string = 'signingVerification';
	static FIELD_decryption: string = 'decryption';

	static getForm(formBuilder: FormBuilder, current: ReceiveSecurity): FormGroup {
		return formBuilder.group({
			signingVerification: SigningVerification.getForm(formBuilder, current && current.signingVerification),
			decryption: Decryption.getForm(formBuilder, current && current.decryption),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceiveSecurity) {

		form.removeControl('signingVerification');
		form.addControl('signingVerification', SigningVerification.getForm(formBuilder, current && current.signingVerification));
		form.removeControl('decryption');
		form.addControl('decryption', Decryption.getForm(formBuilder, current && current.decryption));
	}
}
