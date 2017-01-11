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
		form.get(this.FIELD_value).reset({ value: current && current.value, disabled: !!!current && form.parent.disabled });
		form.get(this.FIELD_type).reset({ value: current && current.type, disabled: !!!current && form.parent.disabled });
		form.get(this.FIELD_pModeId).reset({ value: current && current.pModeId, disabled: !!!current && form.parent.disabled });
	}
}
