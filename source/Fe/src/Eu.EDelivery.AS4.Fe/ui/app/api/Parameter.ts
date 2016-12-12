/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Parameter {
	name: string;
	value: string;

	static FIELD_name: string = 'name';	
	static FIELD_value: string = 'value';	

	static getForm(formBuilder: FormBuilder, current: Parameter): FormGroup {
		return formBuilder.group({
			name: [current && current.name],
			value: [current && current.value],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Parameter) {
		form.removeControl('name');
		form.addControl('name', formBuilder.control(current && current.name));
		form.removeControl('value');
		form.addControl('value', formBuilder.control(current && current.value));

	}
}
