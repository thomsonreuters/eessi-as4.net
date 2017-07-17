/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Setting {
	key: string;
	value: string;
	attributes: string[];

	static FIELD_key: string = 'key';
	static FIELD_value: string = 'value';
	static FIELD_attributes = 'attributes';

	constructor(init?: Partial<Setting>) {
		if (!!init) {
			Object.assign(this, init);
		}
	}
}