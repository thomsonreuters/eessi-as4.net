/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Validators } from '@angular/forms';

export class ReceptionAwareness {
	isEnabled: boolean;
	retryCount: number;
	retryInterval: string;

	static FIELD_isEnabled: string = 'isEnabled';
	static FIELD_retryCount: string = 'retryCount';
	static FIELD_retryInterval: string = 'retryInterval';

	static getForm(formBuilder: FormBuilder, current: ReceptionAwareness): FormGroup {
		let form = formBuilder.group({
			[this.FIELD_isEnabled]: [!!(current && current.isEnabled), Validators.required],
			[this.FIELD_retryCount]: [(current == null || current.retryCount == null) ? 0 : current.retryCount, Validators.required],
			[this.FIELD_retryInterval]: [(current == null || current.retryInterval == null) ? 0 : current.retryInterval, Validators.required],
		});
		ReceptionAwareness.setupForm(form);
		return form;
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceptionAwareness) {
		form.removeControl(this.FIELD_isEnabled);
		form.addControl(this.FIELD_isEnabled, formBuilder.control(current && current.isEnabled));
		form.removeControl(this.FIELD_retryCount);
		form.addControl(this.FIELD_retryCount, formBuilder.control(current && current.retryCount));
		form.removeControl(this.FIELD_retryInterval);
		form.addControl(this.FIELD_retryInterval, formBuilder.control(current && current.retryInterval));
		ReceptionAwareness.setupForm(form);
	}

	static setupForm(form: FormGroup) {
		this.processEnabled(form);
		form.get(this.FIELD_isEnabled).valueChanges.subscribe(result => this.processEnabled(form));
	}

	static processEnabled(form: FormGroup) {
		let isEnabled = form.get(this.FIELD_isEnabled).value;
		if (isEnabled) {
			setTimeout(() => {
				form.get(this.FIELD_retryCount).enable();
				form.get(this.FIELD_retryInterval).enable();
			});
		}
		else {
			setTimeout(() => {
				form.get(this.FIELD_retryCount).disable();
				form.get(this.FIELD_retryInterval).disable();
			});
		}
	}
}
