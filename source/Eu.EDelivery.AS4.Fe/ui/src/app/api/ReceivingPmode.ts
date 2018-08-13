/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { ReceivingProcessingMode } from "./ReceivingProcessingMode";
import { IPmode } from './Pmode.interface';

export class ReceivingPmode implements IPmode {
	type: number;
	name: string;
	hash: string;
	pmode: ReceivingProcessingMode;

	static FIELD_type: string = 'type';
	static FIELD_name: string = 'name';
	static FIELD_pmode: string = 'pmode';
}
