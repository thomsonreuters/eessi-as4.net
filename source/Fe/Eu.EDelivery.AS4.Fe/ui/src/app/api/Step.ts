/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Setting } from "./Setting";

export class Step {
	type: string;
	unDecorated: boolean;
	setting: Setting[];

	static FIELD_type: string = 'type';	
	static FIELD_unDecorated: string = 'unDecorated';	
	static FIELD_setting: string = 'setting';	
}
