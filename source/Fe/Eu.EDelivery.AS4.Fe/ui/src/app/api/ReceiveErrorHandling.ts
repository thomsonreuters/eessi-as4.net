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
}
