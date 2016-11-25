/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

export class Transformer {
	type: string;

	static getForm(formBuilder: FormBuilder, current: Transformer): FormGroup {
		return formBuilder.group({
				type: [current && current.type],
		});
	}
}
