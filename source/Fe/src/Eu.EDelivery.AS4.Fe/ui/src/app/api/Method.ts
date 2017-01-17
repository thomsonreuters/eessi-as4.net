/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Parameter } from "./Parameter";
import { Validators } from '@angular/forms';

export class Method {
	type: string;
	parameters: Parameter[];

	static FIELD_type: string = 'type';
	static FIELD_parameters: string = 'parameters';

	static getForm(formBuilder: FormBuilder, current: Method, isDisabled?: boolean): FormGroup {
		let form = formBuilder.group({
			[this.FIELD_type]: [{ value: current && current.type }, Validators.required],
			[this.FIELD_parameters]: formBuilder.array(!!!(current && current.parameters) ? [] : current.parameters.map(item => Parameter.getForm(formBuilder, item))),
		});
		this.setupForm(form);
		return form;
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Method, disabled?: boolean) {
		form.get(this.FIELD_type).reset({ value: current && current.type, disabled: disabled });
		form.removeControl(this.FIELD_parameters);
		form.addControl(this.FIELD_parameters, formBuilder.array(!!!(current && current.parameters) ? [] : current.parameters.map(item => Parameter.getForm(formBuilder, item, disabled))));
	}
	static setupForm(form: FormGroup) {
		form.get(this.FIELD_type).statusChanges.subscribe(result => {
		});
	}
}
