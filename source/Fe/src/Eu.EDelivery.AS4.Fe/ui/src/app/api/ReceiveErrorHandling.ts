/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class ReceiveErrorHandling {
	useSoapFault: boolean;
	replyPattern: number;
	callbackUrl: string;
	responseHttpCode: number;
	sendingPMode: string;

	static FIELD_useSoapFault: string = 'useSoapFault';
	static FIELD_replyPattern: string = 'replyPattern';
	static FIELD_callbackUrl: string = 'callbackUrl';
	static FIELD_responseHttpCode: string = 'responseHttpCode';
	static FIELD_sendingPMode: string = 'sendingPMode';

	static getForm(formBuilder: FormBuilder, current: ReceiveErrorHandling): FormGroup {
		return formBuilder.group({
			useSoapFault: [!!(current && current.useSoapFault)],
			replyPattern: [(current == null || current.replyPattern == null) ? 0 : current.replyPattern],
			callbackUrl: [current && current.callbackUrl],
			responseHttpCode: [(current == null || current.responseHttpCode == null) ? '200' : current.responseHttpCode],
			sendingPMode: [current && current.sendingPMode],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceiveErrorHandling) {
		form.removeControl('useSoapFault');
		form.addControl('useSoapFault', formBuilder.control(current && current.useSoapFault));
		form.removeControl('replyPattern');
		form.addControl('replyPattern', formBuilder.control(current && current.replyPattern));
		form.removeControl('callbackUrl');
		form.addControl('callbackUrl', formBuilder.control(current && current.callbackUrl));
		form.removeControl('responseHttpCode');
		form.addControl('responseHttpCode', formBuilder.control(current && current.responseHttpCode));
		form.removeControl('sendingPMode');
		form.addControl('sendingPMode', formBuilder.control(current && current.sendingPMode));

	}
}
