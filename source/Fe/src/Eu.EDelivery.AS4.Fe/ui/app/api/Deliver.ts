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
		let form = formBuilder.group({
			isEnabled: [!!(current && current.isEnabled)],
			payloadReferenceMethod: Method.getForm(formBuilder, current && current.payloadReferenceMethod),
			deliverMethod: Method.getForm(formBuilder, current && current.deliverMethod),
		});
		setTimeout(() => Deliver.setupForm(form));
		return form;
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Deliver) {
		form.removeControl('isEnabled');
		form.addControl('isEnabled', formBuilder.control(current && current.isEnabled));

		form.removeControl('payloadReferenceMethod');
		form.addControl('payloadReferenceMethod', Method.getForm(formBuilder, current && current.payloadReferenceMethod));
		form.removeControl('deliverMethod');
		form.addControl('deliverMethod', Method.getForm(formBuilder, current && current.deliverMethod));

		setTimeout(() => Deliver.setupForm(form));
	}

	static setupForm(formGroup: FormGroup) {
		let enable = () => {
			payload.enable({ onlySelf: true });
			method.enable({ onlySelf: true });
		};
		let disable = () => {
			payload.disable({ onlySelf: false });
			method.disable({ onlySelf: false });
		}

		let payload = formGroup.get(Deliver.FIELD_payloadReferenceMethod);
		let method = formGroup.get(Deliver.FIELD_deliverMethod);
		let isEnabled = formGroup.get(Deliver.FIELD_isEnabled);
		if (isEnabled.value) enable();
		else disable();

		isEnabled.valueChanges.subscribe(result => {
			if (!result) disable();
			else enable();
		});
	}
}
