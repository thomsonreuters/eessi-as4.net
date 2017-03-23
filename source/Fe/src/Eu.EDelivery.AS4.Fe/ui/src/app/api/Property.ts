/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Property {
	friendlyName: string;
	type: string;
	regex: string;
	description: string;

	static FIELD_friendlyName: string = 'friendlyName';	
	static FIELD_type: string = 'type';	
	static FIELD_regex: string = 'regex';	
	static FIELD_description: string = 'description';		
}
