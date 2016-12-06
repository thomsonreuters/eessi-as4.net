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
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: Parameter) {
	}
}
