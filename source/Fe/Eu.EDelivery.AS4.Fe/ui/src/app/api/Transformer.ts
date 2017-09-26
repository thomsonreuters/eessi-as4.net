/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Transformer {
	type: string;

	static FIELD_type: string = 'type';

	constructor(init?: Partial<Transformer>) {
		Object.assign(this, init);
	}
}
