/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Validators } from '@angular/forms';

export class Parameter {
	name: string;
	value: string;

	static FIELD_name: string = 'name';
	static FIELD_value: string = 'value';	
}
