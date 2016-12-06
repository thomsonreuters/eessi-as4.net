/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Method } from "./Method";

export class Receivehandling {
	notifyMessageConsumer: boolean;
	notifyMethod: Method;

	static FIELD_notifyMessageConsumer: string = 'notifyMessageConsumer';	
	static FIELD_notifyMethod: string = 'notifyMethod';

	static getForm(formBuilder: FormBuilder, current: Receivehandling): FormGroup {
		return formBuilder.group({
			notifyMessageConsumer: [!!(current && current.notifyMessageConsumer)],
			notifyMethod: Method.getForm(formBuilder, current && current.notifyMethod),
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: Receivehandling) {
		Method.patchFormArrays(formBuilder, <FormGroup>form.controls['notifyMethod'], current && current.notifyMethod);
	}
}
