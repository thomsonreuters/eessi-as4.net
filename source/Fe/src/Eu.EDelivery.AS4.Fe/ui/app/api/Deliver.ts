/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Method } from "./Method";

export class Deliver {
	isEnabled: boolean;
	payloadReferenceMethod: Method;
	deliverMethod: Method;

	static FIELD_isEnabled: string = 'isEnabled';
	static FIELD_payloadReferenceMethod: string = 'payloadReferenceMethod';
	static FIELD_deliverMethod: string = 'deliverMethod';

	static getForm(formBuilder: FormBuilder, current: Deliver): FormGroup {
		return formBuilder.group({
			isEnabled: [!!(current && current.isEnabled)],
			payloadReferenceMethod: Method.getForm(formBuilder, current && current.payloadReferenceMethod),
			deliverMethod: Method.getForm(formBuilder, current && current.deliverMethod),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Deliver) {
		form.removeControl('isEnabled');
		form.addControl('isEnabled', formBuilder.control(current && current.isEnabled));

		form.removeControl('payloadReferenceMethod');
		form.addControl('payloadReferenceMethod', Method.getForm(formBuilder, current && current.payloadReferenceMethod));
		form.removeControl('deliverMethod');
		form.addControl('deliverMethod', Method.getForm(formBuilder, current && current.deliverMethod));
	}
}
