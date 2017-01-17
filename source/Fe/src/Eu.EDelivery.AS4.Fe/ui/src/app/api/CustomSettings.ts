/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Setting } from "./Setting";

export class CustomSettings {
	setting: Setting[];

	static FIELD_setting: string = 'setting';

	static getForm(formBuilder: FormBuilder, current: CustomSettings): FormGroup {
		return formBuilder.group({
			setting: formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => Setting.getForm(formBuilder, item))),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: CustomSettings) {

		form.removeControl('setting');
		form.addControl('setting', formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => Setting.getForm(formBuilder, item))));
	}
}
