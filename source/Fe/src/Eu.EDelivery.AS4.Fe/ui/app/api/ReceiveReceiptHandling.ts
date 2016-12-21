import { FormControl, ControlValueAccessor } from '@angular/forms';
/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Validators } from '@angular/forms';

export class ReceiveReceiptHandling {
	useNNRFormat: boolean;
	replyPattern: number;
	callbackUrl: string;
	sendingPMode: string;

	static FIELD_useNNRFormat: string = 'useNNRFormat';
	static FIELD_replyPattern: string = 'replyPattern';
	static FIELD_callbackUrl: string = 'callbackUrl';
	static FIELD_sendingPMode: string = 'sendingPMode';

	static getForm(formBuilder: FormBuilder, current: ReceiveReceiptHandling): FormGroup {
		let form = formBuilder.group({
			[this.FIELD_useNNRFormat]: [!!(current && current.useNNRFormat), Validators.required],
			[this.FIELD_replyPattern]: [(current == null || current.replyPattern == null) ? 0 : current.replyPattern, Validators.required],
			[this.FIELD_callbackUrl]: [current && current.callbackUrl],
			[this.FIELD_sendingPMode]: [current && current.sendingPMode],
		});
		ReceiveReceiptHandling.setupForm(form);
		return form;
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceiveReceiptHandling) {
		form.removeControl(this.FIELD_useNNRFormat);
		form.addControl(this.FIELD_useNNRFormat, formBuilder.control(current && current.useNNRFormat, Validators.required));
		form.removeControl(this.FIELD_replyPattern);
		form.addControl(this.FIELD_replyPattern, formBuilder.control(current && current.replyPattern, Validators.required));
		form.removeControl(this.FIELD_callbackUrl);
		form.addControl(this.FIELD_callbackUrl, formBuilder.control(current && current.callbackUrl));
		form.removeControl(this.FIELD_sendingPMode);
		form.addControl(this.FIELD_sendingPMode, formBuilder.control(current && current.sendingPMode));
		ReceiveReceiptHandling.setupForm(form);
	}

	static setupForm(form: FormGroup) {
		let patternChanges = form.get(this.FIELD_replyPattern);

		patternChanges.valueChanges.subscribe(result => this.processCallbackUrl(form));
		this.processCallbackUrl(form);
	}

	static processCallbackUrl(form: FormGroup) {
		let callbackUrl = form.get(this.FIELD_callbackUrl);
		let replyPattern = form.get(this.FIELD_replyPattern).value;

		if (+replyPattern === 1) // Callback
		{
			callbackUrl.setValidators(Validators.required);
			setTimeout(() => callbackUrl.enable());
		}
		else {
			callbackUrl.clearValidators();
			setTimeout(() => callbackUrl.disable());
		}
	}
}
