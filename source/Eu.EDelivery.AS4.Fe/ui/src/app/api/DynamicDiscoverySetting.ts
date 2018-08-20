/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Validators } from '@angular/forms';

export class DynamicDiscoverySetting {
	key: string;
	value: string;

	static FIELD_key: string = 'key';
	static FIELD_value: string = 'value';	
}
