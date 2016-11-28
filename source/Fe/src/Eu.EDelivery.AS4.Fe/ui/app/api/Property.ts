/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';


export class Property {
		friendlyName: string;
		type: string;
		regex: string;
		description: string;


	static getForm(formBuilder: FormBuilder, current: Property): FormGroup {
		return formBuilder.group({
			friendlyName: [''],
			type: [''],
			regex: [''],
			description: [''],
		});
	}
}
