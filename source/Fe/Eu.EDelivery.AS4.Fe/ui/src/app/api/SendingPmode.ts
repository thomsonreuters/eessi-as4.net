/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { SendingProcessingMode } from "./SendingProcessingMode";
import { IPmode } from './Pmode.interface';

export class SendingPmode implements IPmode {
	type: number;
	name: string;
	hash: string;
	pmode: SendingProcessingMode;
	isPushConfigurationEnabled: boolean;

	static FIELD_type: string = 'type';
	static FIELD_name: string = 'name';
	static FIELD_pmode: string = 'pmode';
	static FIELD_isPushConfigurationEnabled: string = 'isPushConfigurationEnabled';
}
