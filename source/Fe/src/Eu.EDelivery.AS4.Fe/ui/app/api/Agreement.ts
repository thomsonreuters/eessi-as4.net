/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Agreement {
	value: string;
	type: string;
	pModeId: string;

	static FIELD_value: string = 'value';
	static FIELD_type: string = 'type';
	static FIELD_pModeId: string = 'pModeId';

	static getForm(formBuilder: FormBuilder, current: Agreement): FormGroup {
		return formBuilder.group({
			[this.FIELD_value]: [current && current.value],
			[this.FIELD_type]: [current && current.type],
			[this.FIELD_pModeId]: [current && current.pModeId],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Agreement) {
		form.removeControl(this.FIELD_value);
		form.addControl(this.FIELD_value, formBuilder.control(current && current.value));
		form.removeControl(this.FIELD_type);
		form.addControl(this.FIELD_type, formBuilder.control(current && current.type));
		form.removeControl(this.FIELD_pModeId);
		form.addControl(this.FIELD_pModeId, formBuilder.control(current && current.pModeId));
	}
}
