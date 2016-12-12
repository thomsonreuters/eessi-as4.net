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
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SendingProcessingMode) {
		form.removeControl('id');
		form.addControl('id', formBuilder.control(current && current.id));
		form.removeControl('allowOverride');
		form.addControl('allowOverride', formBuilder.control(current && current.allowOverride));
		form.removeControl('mep');
		form.addControl('mep', formBuilder.control(current && current.mep));
		form.removeControl('mepBinding');
		form.addControl('mepBinding', formBuilder.control(current && current.mepBinding));

		form.removeControl('pushConfiguration');
		form.addControl('pushConfiguration', PushConfiguration.getForm(formBuilder, current && current.pushConfiguration));
		form.removeControl('pullConfiguration');
		form.addControl('pullConfiguration', PullConfiguration.getForm(formBuilder, current && current.pullConfiguration));
		form.removeControl('reliability');
		form.addControl('reliability', SendReliability.getForm(formBuilder, current && current.reliability));
		form.removeControl('receiptHandling');
		form.addControl('receiptHandling', SendHandling.getForm(formBuilder, current && current.receiptHandling));
		form.removeControl('errorHandling');
		form.addControl('errorHandling', SendHandling.getForm(formBuilder, current && current.errorHandling));
		form.removeControl('exceptionHandling');
		form.addControl('exceptionHandling', SendHandling.getForm(formBuilder, current && current.exceptionHandling));
		form.removeControl('security');
		form.addControl('security', Security.getForm(formBuilder, current && current.security));
		form.removeControl('messagePackaging');
		form.addControl('messagePackaging', SendMessagePackaging.getForm(formBuilder, current && current.messagePackaging));
	}
}
