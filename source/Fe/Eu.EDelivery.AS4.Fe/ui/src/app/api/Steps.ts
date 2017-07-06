/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Step } from "./Step";

export class Steps {
	normalPipeline: Step[];
	errorPipeline: Step[];

	static FIELD_normalPipeline: string = 'normalPipeline';
	static FIELD_errorPipeline: string = 'errorPipeline';
}
