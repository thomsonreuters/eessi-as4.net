/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';


export class Setting {
		key: string;
		value: string;


	static getForm(formBuilder: FormBuilder, current: Setting): FormGroup {
		return formBuilder.group({
			key: [''],
			value: [''],
		});
	}
}
