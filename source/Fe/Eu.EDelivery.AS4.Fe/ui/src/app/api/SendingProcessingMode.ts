/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { PushConfiguration } from "./PushConfiguration";
import { PullConfiguration } from "./PullConfiguration";
import { SendReliability } from "./SendReliability";
import { SendHandling } from "./SendHandling";
import { Security } from "./Security";
import { SendMessagePackaging } from "./SendMessagePackaging";
import { MessagePackaging } from './MessagePackaging';
import { DynamicDiscovery } from './DynamicDiscovery';

export class SendingProcessingMode {
	id: string;
	allowOverride: boolean;
	mep: number;
	mepBinding: number;
	pushConfiguration: PushConfiguration = new PushConfiguration();
	pullConfiguration: PullConfiguration = new PullConfiguration();
	dynamicDiscovery: DynamicDiscovery = new DynamicDiscovery();
	reliability: SendReliability = new SendReliability();
	receiptHandling: SendHandling = new SendHandling();
	errorHandling: SendHandling = new SendHandling();
	exceptionHandling: SendHandling = new SendHandling();
	security: Security = new Security();
	messagePackaging: SendMessagePackaging = new SendMessagePackaging();
	
	static FIELD_id: string = 'id';
	static FIELD_allowOverride: string = 'allowOverride';
	static FIELD_mep: string = 'mep';
	static FIELD_mepBinding: string = 'mepBinding';
	static FIELD_pushConfiguration: string = 'pushConfiguration';
	static FIELD_pullConfiguration: string = 'pullConfiguration';
	static FIELD_dynamicDiscovery: string = 'dynamicDiscovery';
	static FIELD_reliability: string = 'reliability';
	static FIELD_receiptHandling: string = 'receiptHandling';
	static FIELD_errorHandling: string = 'errorHandling';
	static FIELD_exceptionHandling: string = 'exceptionHandling';
	static FIELD_security: string = 'security';
	static FIELD_messagePackaging: string = 'messagePackaging';
}
