/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';


export class Property {
	friendlyName: string;
	type: string;
	regex: string;
	description: string;

	static FIELD_friendlyName: string = 'friendlyName';
	static FIELD_type: string = 'type';
	static FIELD_regex: string = 'regex';
	static FIELD_description: string = 'description';

	static getForm(formBuilder: FormBuilder, current: Property): FormGroup {
		return formBuilder.group({
			friendlyName: [current && current.friendlyName],
			type: [current && current.type],
			regex: [current && current.regex],
			description: [current && current.description],
		});
	}
}
