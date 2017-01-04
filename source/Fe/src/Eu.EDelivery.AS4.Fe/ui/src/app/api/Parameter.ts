/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Validators } from '@angular/forms';

export class Parameter {
	name: string;
	value: string;

	static FIELD_name: string = 'name';
	static FIELD_value: string = 'value';

	static getForm(formBuilder: FormBuilder, current: Parameter): FormGroup {
		return formBuilder.group({
			[this.FIELD_name]: [current && current.name, Validators.required],
			[this.FIELD_value]: [current && current.value, Validators.required],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Parameter) {
		form.removeControl(this.FIELD_name);
		form.addControl(this.FIELD_name, formBuilder.control(current && current.name));
		form.removeControl(this.FIELD_value);
		form.addControl(this.FIELD_value, formBuilder.control(current && current.value));
	}
}
