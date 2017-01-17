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
		form.get(this.FIELD_useSoapFault).reset({ value: current && current.useSoapFault, disabled: !!!current && form.parent.disabled });
		form.get(this.FIELD_replyPattern).reset({ value: current && current.replyPattern, disabled: !!!current && form.parent.disabled });
		form.get(this.FIELD_callbackUrl).reset({ value: current && current.callbackUrl, disabled: !!!current && form.parent.disabled });
		form.get(this.FIELD_responseHttpCode).reset({ value: current && current.responseHttpCode, disabled: !!!current && form.parent.disabled });
		form.get(this.FIELD_sendingPMode).reset({ value: current && current.sendingPMode, disabled: !!!current && form.parent.disabled });
	}
}
