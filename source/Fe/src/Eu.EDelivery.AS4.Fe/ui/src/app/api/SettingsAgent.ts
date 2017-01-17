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
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SettingsAgent) {
		form.removeControl('name');
		form.addControl('name', formBuilder.control(current && current.name));

		form.removeControl('receiver');
		form.addControl('receiver', Receiver.getForm(formBuilder, current && current.receiver));
		form.removeControl('transformer');
		form.addControl('transformer', Transformer.getForm(formBuilder, current && current.transformer));
		form.removeControl('steps');
		form.addControl('steps', Steps.getForm(formBuilder, current && current.steps));
	}
}
