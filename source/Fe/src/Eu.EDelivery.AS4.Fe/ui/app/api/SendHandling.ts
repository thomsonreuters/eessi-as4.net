/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Method } from "./Method";

export class SendHandling {
	notifyMessageProducer: boolean;
	notifyMethod: Method;

	static FIELD_notifyMessageProducer: string = 'notifyMessageProducer';	
	static FIELD_notifyMethod: string = 'notifyMethod';

	static getForm(formBuilder: FormBuilder, current: SendHandling): FormGroup {
		return formBuilder.group({
			notifyMessageProducer: [!!(current && current.notifyMessageProducer)],
			notifyMethod: Method.getForm(formBuilder, current && current.notifyMethod),
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: SendHandling) {
		Method.patchFormArrays(formBuilder, <FormGroup>form.controls['notifyMethod'], current && current.notifyMethod);
	}
}
