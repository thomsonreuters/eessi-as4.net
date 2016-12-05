/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Setting {
	key: string;
	value: string;

	static FIELD_key: string = 'key';	
	static FIELD_value: string = 'value';	

	static getForm(formBuilder: FormBuilder, current: Setting): FormGroup {
		return formBuilder.group({
			key: [current && current.key],
			value: [current && current.value],
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: Setting) {
	}
}
