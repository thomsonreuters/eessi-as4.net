/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { SettingsAgent } from "./SettingsAgent";

export class SettingsAgents {
	submitAgents = new Array<SettingsAgent>();
	receiveAgents = new Array<SettingsAgent>();
	sendAgents = new Array<SettingsAgent>();
	deliverAgents = new Array<SettingsAgent>();
	notifyAgents = new Array<SettingsAgent>();
	receptionAwarenessAgent: SettingsAgent;

	static FIELD_submitAgents: string = 'submitAgents';
	static FIELD_receiveAgents: string = 'receiveAgents';
	static FIELD_sendAgents: string = 'sendAgents';
	static FIELD_deliverAgents: string = 'deliverAgents';
	static FIELD_notifyAgents: string = 'notifyAgents';
	static FIELD_receptionAwarenessAgent: string = 'receptionAwarenessAgent';
}
