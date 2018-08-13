/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Receiver } from "./Receiver";
import { Transformer } from "./Transformer";
import { Steps } from "./Steps";

export class SettingsAgent {
	name: string;
	receiver: Receiver;
	transformer: Transformer;
	stepConfiguration: Steps;

	static FIELD_name: string = 'name';	
	static FIELD_receiver: string = 'receiver';
	static FIELD_transformer: string = 'transformer';
	static FIELD_stepConfiguration: string = 'stepConfiguration';
}
