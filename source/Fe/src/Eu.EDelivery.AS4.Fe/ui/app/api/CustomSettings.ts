/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

import { Setting } from "./Setting"

export class CustomSettings {

		setting: Setting[];

	static getForm(formBuilder: FormBuilder, current: CustomSettings): FormGroup {
		return formBuilder.group({
			setting: formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => Setting.getForm(formBuilder, item))),
		});
	}
}
