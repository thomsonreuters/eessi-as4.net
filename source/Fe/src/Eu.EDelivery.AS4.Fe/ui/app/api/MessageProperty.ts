/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class MessageProperty {
	name: string;
	type: string;
	value: string;

	static FIELD_name: string = 'name';	
	static FIELD_type: string = 'type';	
	static FIELD_value: string = 'value';	

	static getForm(formBuilder: FormBuilder, current: MessageProperty): FormGroup {
		return formBuilder.group({
			name: [current && current.name],
			type: [current && current.type],
			value: [current && current.value],
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: MessageProperty) {
	}
}
