/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

import { Steps } from "./Steps"

export class Decorator {
		type: string;

		steps: Steps;

	static getForm(formBuilder: FormBuilder, current: Decorator): FormGroup {
		return formBuilder.group({
			type: [''],
			steps: Steps.getForm(formBuilder, current.steps),
		});
	}
}
