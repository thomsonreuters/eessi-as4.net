/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class MessageProperty {
	name: string;
	type: string;
	value: string;

	static FIELD_name: string = 'name';	
	static FIELD_type: string = 'type';	
	static FIELD_value: string = 'value';	
}
