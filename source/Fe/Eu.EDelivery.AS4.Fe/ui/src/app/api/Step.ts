/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Setting } from "./Setting";

export class Step {
	type: string;
	setting: Setting[];

	static FIELD_type: string = 'type';	
	static FIELD_setting: string = 'setting';	
}
