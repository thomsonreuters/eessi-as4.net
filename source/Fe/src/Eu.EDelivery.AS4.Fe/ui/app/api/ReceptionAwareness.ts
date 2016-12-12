/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class ReceptionAwareness {
	isEnabled: boolean;
	retryCount: number;
	retryInterval: string;

	static FIELD_isEnabled: string = 'isEnabled';	
	static FIELD_retryCount: string = 'retryCount';	
	static FIELD_retryInterval: string = 'retryInterval';	

	static getForm(formBuilder: FormBuilder, current: ReceptionAwareness): FormGroup {
		return formBuilder.group({
			isEnabled: [!!(current && current.isEnabled)],
			retryCount: [current && current.retryCount],
			retryInterval: [current && current.retryInterval],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceptionAwareness) {
		form.removeControl('isEnabled');
		form.addControl('isEnabled', formBuilder.control(current && current.isEnabled));
		form.removeControl('retryCount');
		form.addControl('retryCount', formBuilder.control(current && current.retryCount));
		form.removeControl('retryInterval');
		form.addControl('retryInterval', formBuilder.control(current && current.retryInterval));

	}
}
