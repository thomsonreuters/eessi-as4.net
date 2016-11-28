/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

import { Setting } from "./Setting"

export class Receiver {
		text: Array<string>;
		type: string;

		setting: Setting[];

	static getForm(formBuilder: FormBuilder, current: Receiver): FormGroup {
		return formBuilder.group({
			text: formBuilder.array(!!!(current && current.text) ? [] : current.text),
			type: [current && current.type],
			setting: formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => Setting.getForm(formBuilder, item))),
		});
	}
}
