/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { SettingsAgent } from "./SettingsAgent";

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
			submitAgents: formBuilder.array(!!!(current && current.submitAgents) ? [] : current.submitAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			receiveAgents: formBuilder.array(!!!(current && current.receiveAgents) ? [] : current.receiveAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			sendAgents: formBuilder.array(!!!(current && current.sendAgents) ? [] : current.sendAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			deliverAgents: formBuilder.array(!!!(current && current.deliverAgents) ? [] : current.deliverAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			notifyAgents: formBuilder.array(!!!(current && current.notifyAgents) ? [] : current.notifyAgents.map(item => SettingsAgent.getForm(formBuilder, item))),
			receptionAwarenessAgent: SettingsAgent.getForm(formBuilder, current && current.receptionAwarenessAgent),
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: SettingsAgents) {
		form.removeControl('submitAgents');
		form.addControl('submitAgents', formBuilder.array(!!!(current && current.submitAgents) ? [] : current.submitAgents.map(item => SettingsAgent.getForm(formBuilder, item))),);
		form.removeControl('receiveAgents');
		form.addControl('receiveAgents', formBuilder.array(!!!(current && current.receiveAgents) ? [] : current.receiveAgents.map(item => SettingsAgent.getForm(formBuilder, item))),);
		form.removeControl('sendAgents');
		form.addControl('sendAgents', formBuilder.array(!!!(current && current.sendAgents) ? [] : current.sendAgents.map(item => SettingsAgent.getForm(formBuilder, item))),);
		form.removeControl('deliverAgents');
		form.addControl('deliverAgents', formBuilder.array(!!!(current && current.deliverAgents) ? [] : current.deliverAgents.map(item => SettingsAgent.getForm(formBuilder, item))),);
		form.removeControl('notifyAgents');
		form.addControl('notifyAgents', formBuilder.array(!!!(current && current.notifyAgents) ? [] : current.notifyAgents.map(item => SettingsAgent.getForm(formBuilder, item))),);
		SettingsAgent.patchFormArrays(formBuilder, <FormGroup>form.controls['receptionAwarenessAgent'], current && current.receptionAwarenessAgent);
	}
}
