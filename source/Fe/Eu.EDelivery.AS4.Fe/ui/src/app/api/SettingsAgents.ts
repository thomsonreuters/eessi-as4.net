/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { SettingsAgent } from "./SettingsAgent";

export class SettingsAgents {
	submitAgents = new Array<SettingsAgent>();
	outboundProcessingAgents = new Array<SettingsAgent>();
	sendAgents = new Array<SettingsAgent>();
	receiveAgents = new Array<SettingsAgent>();
	deliverAgents = new Array<SettingsAgent>();
	notifyAgents = new Array<SettingsAgent>();
	notifyConsumerAgents = new Array<SettingsAgent>();
	notifyProducerAgents = new Array<SettingsAgent>();
	receptionAwarenessAgent: SettingsAgent | undefined;
	pullReceiveAgents = new Array<SettingsAgent>();
	pullSendAgents = new Array<SettingsAgent>();

	static FIELD_submitAgents: string = 'submitAgents';
	static FIELD_outboundProcessingAgents: string = 'outboundProcessingAgents';
	static FIELD_sendAgents: string = 'sendAgents';
	static FIELD_receiveAgents: string = 'receiveAgents';
	static FIELD_deliverAgents: string = 'deliverAgents';
	static FIELD_notifyAgents: string = 'notifyAgents';
	static FIELD_notifyConsumerAgents: string = 'notifyConsumerAgents';
	static FIELD_notifyProducerAgents: string = 'notifyProducerAgents';
	static FIELD_receptionAwarenessAgent: string = 'receptionAwarenessAgent';
	static FIELD_pullReceiveAgents: string = 'pullReceiveAgents';
	static FIELD_pullSendAgents: string = 'pullSendAgents';
}
