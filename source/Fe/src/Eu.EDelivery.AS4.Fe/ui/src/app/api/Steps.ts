/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Step } from "./Step";

export class Steps {
	decorator: string;
	step: Step[];

	static FIELD_decorator: string = 'decorator';	
	static FIELD_step: string = 'step';	
}
