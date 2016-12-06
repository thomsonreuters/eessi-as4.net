/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Parameter } from "./Parameter";

export class Method {
	type: string;
	parameters: Parameter[];

	static FIELD_type: string = 'type';	
	static FIELD_parameters: string = 'parameters';

	static getForm(formBuilder: FormBuilder, current: Method): FormGroup {
		return formBuilder.group({
			type: [current && current.type],
			parameters: formBuilder.array(!!!(current && current.parameters) ? [] : current.parameters.map(item => Parameter.getForm(formBuilder, item))),
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: Method) {
		form.removeControl('parameters');
		form.addControl('parameters', formBuilder.array(!!!(current && current.parameters) ? [] : current.parameters.map(item => Parameter.getForm(formBuilder, item))),);
	}
}
