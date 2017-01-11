/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { PushConfiguration } from "./PushConfiguration";
import { PullConfiguration } from "./PullConfiguration";
import { SendReliability } from "./SendReliability";
import { SendHandling } from "./SendHandling";
import { Security } from "./Security";
import { SendMessagePackaging } from "./SendMessagePackaging";
import { MessagePackaging } from './MessagePackaging';

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
			mep: [(current == null || current.mep == null) ? 0 : current.mep],
			mepBinding: [(current == null || current.mepBinding == null) ? 1 : current.mepBinding],
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
		form.get(this.FIELD_id).reset({ value: current && current.id, disabled: !!current || form.parent.disabled });
		form.get(this.FIELD_allowOverride).reset({ value: current && current.allowOverride, disabled: !!current || form.parent.disabled });
		form.get(this.FIELD_mep).reset({ value: current && current.mep, disabled: !!current || form.parent.disabled });
		form.get(this.FIELD_mepBinding).reset({ value: current && current.mepBinding, disabled: !!current || form.parent.disabled });

		PushConfiguration.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_pushConfiguration), current && current.pushConfiguration);
		PullConfiguration.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_pullConfiguration), current && current.pullConfiguration);
		SendReliability.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_reliability), current && current.reliability);
		SendHandling.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_receiptHandling), current && current.receiptHandling);
		SendHandling.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_exceptionHandling), current && current.exceptionHandling);
		Security.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_security), current && current.security);
		MessagePackaging.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_messagePackaging), current && current.messagePackaging);
	}
}
