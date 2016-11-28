/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

import { SettingsAgent } from "./SettingsAgent"

export class SettingsAgents {

	subtmitAgents: SettingsAgent[];
	receiveAgents: SettingsAgent[];
	sendAgents: SettingsAgent[];
	deliverAgents: SettingsAgent[];
	notifyAgents: SettingsAgent[];
	receptionAwarenessAgent: SettingsAgent;

	static getForm(formBuilder: FormBuilder, current: SettingsAgents): FormGroup {
		return formBuilder.group({
			subtmitAgents: formBuilder.array(!!!(current && current.subtmitAgents) ? [] : current.subtmitAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			receiveAgents: formBuilder.array(!!!(current && current.receiveAgents) ? [] : current.receiveAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			sendAgents: formBuilder.array(!!!(current && current.sendAgents) ? [] : current.sendAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			deliverAgents: formBuilder.array(!!!(current && current.deliverAgents) ? [] : current.deliverAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			notifyAgents: formBuilder.array(!!!(current && current.notifyAgents) ? [] : current.notifyAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			receptionAwarenessAgent: SettingsAgent.getForm(formBuilder, current.receptionAwarenessAgent),
		});
	}
}
