/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Method } from "./Method";

export class SendHandling {
	notifyMessageProducer: boolean;
	notifyMethod: Method;

	static FIELD_notifyMessageProducer: string = 'notifyMessageProducer';
	static FIELD_notifyMethod: string = 'notifyMethod';

	static getForm(formBuilder: FormBuilder, current: SendHandling): FormGroup {
		let form = formBuilder.group({
			[this.FIELD_notifyMessageProducer]: [!!(current && current.notifyMessageProducer)],
			[this.FIELD_notifyMethod]: Method.getForm(formBuilder, current && current.notifyMethod),
		});
		setTimeout(() => this.setupForm(form));
		return form;
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SendHandling) {
		form.get(this.FIELD_notifyMessageProducer).reset({ value: current && current.notifyMessageProducer, disabled: !!!current });
		Method.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_notifyMethod), current && current.notifyMethod);
		this.setupForm(form);
	}

	static setupForm(form: FormGroup) {
		let notifyProducer = form.get(this.FIELD_notifyMessageProducer);
		let notifyMethod = form.get(this.FIELD_notifyMethod);
		let enable = () => notifyMethod.enable();
		let disable = () => notifyMethod.disable();
		let toggle = (result: boolean) => {
			if (result) enable();
			else disable();
		};

		toggle(notifyProducer.value);
		notifyProducer.valueChanges.subscribe(result => toggle(result));
	}
}
