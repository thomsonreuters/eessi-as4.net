/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Parameter } from "./Parameter";
import { Validators } from '@angular/forms';

export class Method {
	type: string;
	parameters: Parameter[];

	static FIELD_type: string = 'type';
	static FIELD_parameters: string = 'parameters';
}
