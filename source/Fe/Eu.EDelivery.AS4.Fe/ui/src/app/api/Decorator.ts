/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

import { Steps } from "./Steps"

export class Decorator {
	type: string;
	steps: Steps;

	static FIELD_type: string = 'type';
	static FIELD_steps: string = 'steps';
}
