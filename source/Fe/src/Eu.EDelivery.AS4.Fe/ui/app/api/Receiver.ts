/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

import { Setting } from "./Setting"

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
}
