/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { PushConfiguration } from "./PushConfiguration";
import { PullConfiguration } from "./PullConfiguration";
import { SendReliability } from "./SendReliability";
import { SendHandling } from "./SendHandling";
import { Security } from "./Security";
import { SendMessagePackaging } from "./SendMessagePackaging";

export class SendingProcessingMode {
	id: string;
	allowOverride: boolean;
	mep: number;
	mepBinding: number;
	pushConfiguration: PushConfiguration;
	pullConfiguration: PullConfiguration;
	reliability: SendReliability;
	receiptHandling: SendHandling;
	errorHandling: SendHandling;
	exceptionHandling: SendHandling;
	security: Security;
	messagePackaging: SendMessagePackaging;

	static FIELD_id: string = 'id';
	static FIELD_allowOverride: string = 'allowOverride';
	static FIELD_mep: string = 'mep';
	static FIELD_mepBinding: string = 'mepBinding';
	static FIELD_pushConfiguration: string = 'pushConfiguration';
	static FIELD_pullConfiguration: string = 'pullConfiguration';
	static FIELD_reliability: string = 'reliability';
	static FIELD_receiptHandling: string = 'receiptHandling';
	static FIELD_errorHandling: string = 'errorHandling';
	static FIELD_exceptionHandling: string = 'exceptionHandling';
	static FIELD_security: string = 'security';
	static FIELD_messagePackaging: string = 'messagePackaging';

	static getForm(formBuilder: FormBuilder, current: SendingProcessingMode): FormGroup {
		return formBuilder.group({
			id: [current && current.id],
			allowOverride: [!!(current && current.allowOverride)],
			mep: [current && current.mep],
			mepBinding: [current && current.mepBinding],
			pushConfiguration: PushConfiguration.getForm(formBuilder, current && current.pushConfiguration),
			pullConfiguration: PullConfiguration.getForm(formBuilder, current && current.pullConfiguration),
			reliability: SendReliability.getForm(formBuilder, current && current.reliability),
			receiptHandling: SendHandling.getForm(formBuilder, current && current.receiptHandling),
			errorHandling: SendHandling.getForm(formBuilder, current && current.errorHandling),
			exceptionHandling: SendHandling.getForm(formBuilder, current && current.exceptionHandling),
			security: Security.getForm(formBuilder, current && current.security),
			messagePackaging: SendMessagePackaging.getForm(formBuilder, current && current.messagePackaging),
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: SendingProcessingMode) {
		PushConfiguration.patchFormArrays(formBuilder, <FormGroup>form.controls['pushConfiguration'], current && current.pushConfiguration);
		PullConfiguration.patchFormArrays(formBuilder, <FormGroup>form.controls['pullConfiguration'], current && current.pullConfiguration);
		SendReliability.patchFormArrays(formBuilder, <FormGroup>form.controls['reliability'], current && current.reliability);
		SendHandling.patchFormArrays(formBuilder, <FormGroup>form.controls['receiptHandling'], current && current.receiptHandling);
		SendHandling.patchFormArrays(formBuilder, <FormGroup>form.controls['errorHandling'], current && current.errorHandling);
		SendHandling.patchFormArrays(formBuilder, <FormGroup>form.controls['exceptionHandling'], current && current.exceptionHandling);
		Security.patchFormArrays(formBuilder, <FormGroup>form.controls['security'], current && current.security);
		SendMessagePackaging.patchFormArrays(formBuilder, <FormGroup>form.controls['messagePackaging'], current && current.messagePackaging);
	}
}
