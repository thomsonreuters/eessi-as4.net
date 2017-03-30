/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Agreement {
	value: string;
	type: string;
	pModeId: string;

	static FIELD_value: string = 'value';
	static FIELD_type: string = 'type';
	static FIELD_pModeId: string = 'pModeId';
}
