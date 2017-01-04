/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Setting } from "./Setting";

export class Receiver {
	type: string;
	setting: Setting[];

	static FIELD_type: string = 'type';	
	static FIELD_setting: string = 'setting';

	static getForm(formBuilder: FormBuilder, current: Receiver): FormGroup {
		return formBuilder.group({
			type: [current && current.type],
			setting: formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => Setting.getForm(formBuilder, item))),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Receiver) {
		form.removeControl('type');
		form.addControl('type', formBuilder.control(current && current.type));

		form.removeControl('setting');
		form.addControl('setting', formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => Setting.getForm(formBuilder, item))));
	}
}
