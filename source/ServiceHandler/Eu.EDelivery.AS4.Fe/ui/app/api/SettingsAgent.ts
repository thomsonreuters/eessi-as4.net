/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';
import { Receiver } from './Receiver'
import { Transformer } from './Transformer'
import { Steps } from './Steps'
import { Decorator } from './Decorator'

export class SettingsAgent {
	name: string;
	receiver: Receiver;
	transformer: Transformer;
	steps: Steps;
	decorator: Decorator;

	static getForm(formBuilder: FormBuilder, current: SettingsAgent): FormGroup {
		return formBuilder.group({
			name: [current === null ? '' : current.name],
			receiver: Receiver.getForm(formBuilder, current && current.receiver),
			transformer: Transformer.getForm(formBuilder, current && current.transformer),
			steps: Steps.getForm(formBuilder, current && current.steps),
			decorator: Decorator.getForm(formBuilder, current && current.decorator),
		});
	}
}
