/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class SigningVerification {
	signature: number;

	static FIELD_signature: string = 'signature';

	static getForm(formBuilder: FormBuilder, current: SigningVerification): FormGroup {
		return formBuilder.group({
			signature: [(current == null || current.signature == null) ? 0 : current.signature],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SigningVerification) {
		form.get(this.FIELD_signature).reset({ value: current && current.signature, disabled: !!!current });
	}
}
