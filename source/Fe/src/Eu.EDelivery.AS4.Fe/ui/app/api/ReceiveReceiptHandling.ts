/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

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
		return formBuilder.group({
			useNNRFormat: [!!(current && current.useNNRFormat)],
			replyPattern: [current && current.replyPattern],
			callbackUrl: [current && current.callbackUrl],
			sendingPMode: [current && current.sendingPMode],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceiveReceiptHandling) {
		form.removeControl('useNNRFormat');
		form.addControl('useNNRFormat', formBuilder.control(current && current.useNNRFormat));
		form.removeControl('replyPattern');
		form.addControl('replyPattern', formBuilder.control(current && current.replyPattern));
		form.removeControl('callbackUrl');
		form.addControl('callbackUrl', formBuilder.control(current && current.callbackUrl));
		form.removeControl('sendingPMode');
		form.addControl('sendingPMode', formBuilder.control(current && current.sendingPMode));

	}
}
