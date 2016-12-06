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
			replyPattern: [current && current.replyPattern],
			callbackUrl: [current && current.callbackUrl],
			responseHttpCode: [current && current.responseHttpCode],
			sendingPMode: [current && current.sendingPMode],
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: ReceiveErrorHandling) {
	}
}
