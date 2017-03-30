/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Validators } from '@angular/forms';

export class ReceptionAwareness {
	isEnabled: boolean;
	retryCount: number;
	retryInterval: string;

	static FIELD_isEnabled: string = 'isEnabled';
	static FIELD_retryCount: string = 'retryCount';
	static FIELD_retryInterval: string = 'retryInterval';	
}
