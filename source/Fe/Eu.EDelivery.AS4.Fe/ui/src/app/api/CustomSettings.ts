/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Setting } from "./Setting";

export class CustomSettings {
	setting: Setting[];

	static FIELD_setting: string = 'setting';	
}
