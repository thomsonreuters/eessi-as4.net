/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { ReceiveReliability } from "./ReceiveReliability";
import { ReceiveReceiptHandling } from "./ReceiveReceiptHandling";
import { ReceiveErrorHandling } from "./ReceiveErrorHandling";
import { Receivehandling } from "./Receivehandling";
import { ReceiveSecurity } from "./ReceiveSecurity";
import { MessagePackaging } from "./MessagePackaging";
import { Deliver } from "./Deliver";

export class ReceivingProcessingMode {
	id: string;
	mep: number;
	mepBinding: number;
	reliability: ReceiveReliability;
	receiptHandling: ReceiveReceiptHandling;
	errorHandling: ReceiveErrorHandling;
	exceptionHandling: Receivehandling;
	security: ReceiveSecurity;
	messagePackaging: MessagePackaging;
	deliver: Deliver;

	static FIELD_id: string = 'name';	
	static FIELD_mep: string = 'mep';	
	static FIELD_mepBinding: string = 'mepBinding';	
	static FIELD_reliability: string = 'reliability';
	static FIELD_receiptHandling: string = 'receiptHandling';
	static FIELD_errorHandling: string = 'errorHandling';
	static FIELD_exceptionHandling: string = 'exceptionHandling';
	static FIELD_security: string = 'security';
	static FIELD_messagePackaging: string = 'messagePackaging';
	static FIELD_deliver: string = 'deliver';

	static getForm(formBuilder: FormBuilder, current: ReceivingProcessingMode): FormGroup {
		return formBuilder.group({
			id: [current && current.id],
			mep: [current && current.mep],
			mepBinding: [current && current.mepBinding],
			reliability: ReceiveReliability.getForm(formBuilder, current && current.reliability),
			receiptHandling: ReceiveReceiptHandling.getForm(formBuilder, current && current.receiptHandling),
			errorHandling: ReceiveErrorHandling.getForm(formBuilder, current && current.errorHandling),
			exceptionHandling: Receivehandling.getForm(formBuilder, current && current.exceptionHandling),
			security: ReceiveSecurity.getForm(formBuilder, current && current.security),
			messagePackaging: MessagePackaging.getForm(formBuilder, current && current.messagePackaging),
			deliver: Deliver.getForm(formBuilder, current && current.deliver),
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: ReceivingProcessingMode) {
		ReceiveReliability.patchFormArrays(formBuilder, <FormGroup>form.controls['reliability'], current && current.reliability);
		ReceiveReceiptHandling.patchFormArrays(formBuilder, <FormGroup>form.controls['receiptHandling'], current && current.receiptHandling);
		ReceiveErrorHandling.patchFormArrays(formBuilder, <FormGroup>form.controls['errorHandling'], current && current.errorHandling);
		Receivehandling.patchFormArrays(formBuilder, <FormGroup>form.controls['exceptionHandling'], current && current.exceptionHandling);
		ReceiveSecurity.patchFormArrays(formBuilder, <FormGroup>form.controls['security'], current && current.security);
		MessagePackaging.patchFormArrays(formBuilder, <FormGroup>form.controls['messagePackaging'], current && current.messagePackaging);
		Deliver.patchFormArrays(formBuilder, <FormGroup>form.controls['deliver'], current && current.deliver);
	}
}
