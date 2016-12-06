/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Service {
	value: string;
	type: string;

	static FIELD_value: string = 'value';	
	static FIELD_type: string = 'type';	

	static getForm(formBuilder: FormBuilder, current: Service): FormGroup {
		return formBuilder.group({
			value: [current && current.value],
			type: [current && current.type],
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: Service) {
	}
}
