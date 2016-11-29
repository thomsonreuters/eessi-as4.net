/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';


export class Setting {
	key: string;
	value: string;

	static FIELD_key: string = 'key';
	static FIELD_value: string = 'value';

	static getForm(formBuilder: FormBuilder, current: Setting): FormGroup {
		return formBuilder.group({
			key: [current && current.key],
			value: [current && current.value],
		});
	}
}
