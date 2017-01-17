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
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Setting) {
		form.removeControl('key');
		form.addControl('key', formBuilder.control(current && current.key));
		form.removeControl('value');
		form.addControl('value', formBuilder.control(current && current.value));

	}
}
