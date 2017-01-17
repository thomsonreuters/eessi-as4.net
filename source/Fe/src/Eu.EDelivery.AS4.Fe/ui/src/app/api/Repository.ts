/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Repository {
	type: string;

	static FIELD_type: string = 'type';

	static getForm(formBuilder: FormBuilder, current: Repository): FormGroup {
		return formBuilder.group({
			type: [current && current.type],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Repository) {
		form.get(this.FIELD_type).reset({ value: current && current.type });
	}
}
