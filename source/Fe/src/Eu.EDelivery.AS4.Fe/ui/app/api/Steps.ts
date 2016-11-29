/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

import { Step } from "./Step"

export class Steps {
	decorator: string;
	step: Step[];

	static FIELD_decorator: string = 'decorator';
	static FIELD_step: string = 'step';

	static getForm(formBuilder: FormBuilder, current: Steps): FormGroup {
		return formBuilder.group({
			decorator: [current && current.decorator],
			step: formBuilder.array(!!!(current && current.step) ? [] : current.step.map(item => Step.getForm(formBuilder, item))),
		});
	}
}
