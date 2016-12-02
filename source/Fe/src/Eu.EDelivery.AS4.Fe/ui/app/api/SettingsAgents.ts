/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

import { SettingsAgent } from "./SettingsAgent"

export class SettingsAgents {
	submitAgents: SettingsAgent[];
	receiveAgents: SettingsAgent[];
	sendAgents: SettingsAgent[];
	deliverAgents: SettingsAgent[];
	notifyAgents: SettingsAgent[];
	receptionAwarenessAgent: SettingsAgent;

	static FIELD_submitAgents: string = 'submitAgents';
	static FIELD_receiveAgents: string = 'receiveAgents';
	static FIELD_sendAgents: string = 'sendAgents';
	static FIELD_deliverAgents: string = 'deliverAgents';
	static FIELD_notifyAgents: string = 'notifyAgents';
	static FIELD_receptionAwarenessAgent: string = 'receptionAwarenessAgent';

	static getForm(formBuilder: FormBuilder, current: SettingsAgents): FormGroup {
		return formBuilder.group({
			subtmitAgents: formBuilder.array(!!!(current && current.submitAgents) ? [] : current.submitAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			receiveAgents: formBuilder.array(!!!(current && current.receiveAgents) ? [] : current.receiveAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			sendAgents: formBuilder.array(!!!(current && current.sendAgents) ? [] : current.sendAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			deliverAgents: formBuilder.array(!!!(current && current.deliverAgents) ? [] : current.deliverAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			notifyAgents: formBuilder.array(!!!(current && current.notifyAgents) ? [] : current.notifyAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			receptionAwarenessAgent: SettingsAgent.getForm(formBuilder, current && current.receptionAwarenessAgent),
		});
	}
}
