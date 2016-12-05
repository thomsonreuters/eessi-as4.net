/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Setting } from "./Setting";

export class Step {
	type: string;
	unDecorated: boolean;
	setting: Setting[];

	static FIELD_type: string = 'type';	
	static FIELD_unDecorated: string = 'unDecorated';	
	static FIELD_setting: string = 'setting';

	static getForm(formBuilder: FormBuilder, current: Step): FormGroup {
		return formBuilder.group({
			type: [current && current.type],
			unDecorated: [!!(current && current.unDecorated)],
			setting: formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => Setting.getForm(formBuilder, item))),
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: Step) {
		form.removeControl('setting');
		form.addControl('setting', formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => Setting.getForm(formBuilder, item))),);
	}
}
