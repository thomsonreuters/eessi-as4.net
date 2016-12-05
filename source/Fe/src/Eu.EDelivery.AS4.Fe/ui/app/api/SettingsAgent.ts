/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Receiver } from "./Receiver";
import { Transformer } from "./Transformer";
import { Steps } from "./Steps";

export class SettingsAgent {
	name: string;
	receiver: Receiver;
	transformer: Transformer;
	steps: Steps;

	static FIELD_name: string = 'name';	
	static FIELD_receiver: string = 'receiver';
	static FIELD_transformer: string = 'transformer';
	static FIELD_steps: string = 'steps';

	static getForm(formBuilder: FormBuilder, current: SettingsAgent): FormGroup {
		return formBuilder.group({
			name: [current && current.name],
			receiver: Receiver.getForm(formBuilder, current && current.receiver),
			transformer: Transformer.getForm(formBuilder, current && current.transformer),
			steps: Steps.getForm(formBuilder, current && current.steps),
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: SettingsAgent) {
		Receiver.patchFormArrays(formBuilder, <FormGroup>form.controls['receiver'], current && current.receiver);
		Transformer.patchFormArrays(formBuilder, <FormGroup>form.controls['transformer'], current && current.transformer);
		Steps.patchFormArrays(formBuilder, <FormGroup>form.controls['steps'], current && current.steps);
	}
}
