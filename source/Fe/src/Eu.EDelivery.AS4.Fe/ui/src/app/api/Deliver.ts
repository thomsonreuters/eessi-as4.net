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
			isEnabled: [{ value: !!(current && current.isEnabled), disabled: !!current }],
			payloadReferenceMethod: Method.getForm(formBuilder, current && current.payloadReferenceMethod),
			deliverMethod: Method.getForm(formBuilder, current && current.deliverMethod),
		});
		Deliver.setupForm(form);
		return form;
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Deliver) {
		form.get(this.FIELD_isEnabled).reset({ value: current && current.isEnabled, disabled: !!!current && form.parent.disabled });
		Method.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_deliverMethod), current && current.deliverMethod, current && !current.isEnabled);
		Method.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_payloadReferenceMethod), current && current.payloadReferenceMethod, current && !current.isEnabled);
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
		if (isEnabled.value) {
			payload.enable();
		}
		else {
			payload.disable();
		}
		isEnabled
			.valueChanges
			.filter(() => !(formGroup && formGroup.parent && formGroup.parent.disabled))
			.subscribe(result => {
				if (!result) disable();
				else enable();
				formGroup.markAsDirty();
			});
	}
}
