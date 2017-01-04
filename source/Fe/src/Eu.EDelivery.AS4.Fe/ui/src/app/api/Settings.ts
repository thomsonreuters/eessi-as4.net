/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { SettingsDatabase } from "./SettingsDatabase";
import { CertificateStore } from "./CertificateStore";
import { CustomSettings } from "./CustomSettings";
import { SettingsAgents } from "./SettingsAgents";

export class Settings {
	idFormat: string;
	database: SettingsDatabase;
	certificateStore: CertificateStore;
	customSettings: CustomSettings;
	agents: SettingsAgents;

	static FIELD_idFormat: string = 'idFormat';	
	static FIELD_database: string = 'database';
	static FIELD_certificateStore: string = 'certificateStore';
	static FIELD_customSettings: string = 'customSettings';
	static FIELD_agents: string = 'agents';

	static getForm(formBuilder: FormBuilder, current: Settings): FormGroup {
		return formBuilder.group({
			idFormat: [current && current.idFormat],
			database: SettingsDatabase.getForm(formBuilder, current && current.database),
			certificateStore: CertificateStore.getForm(formBuilder, current && current.certificateStore),
			customSettings: CustomSettings.getForm(formBuilder, current && current.customSettings),
			agents: SettingsAgents.getForm(formBuilder, current && current.agents),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Settings) {
		form.removeControl('idFormat');
		form.addControl('idFormat', formBuilder.control(current && current.idFormat));

		form.removeControl('database');
		form.addControl('database', SettingsDatabase.getForm(formBuilder, current && current.database));
		form.removeControl('certificateStore');
		form.addControl('certificateStore', CertificateStore.getForm(formBuilder, current && current.certificateStore));
		form.removeControl('customSettings');
		form.addControl('customSettings', CustomSettings.getForm(formBuilder, current && current.customSettings));
		form.removeControl('agents');
		form.addControl('agents', SettingsAgents.getForm(formBuilder, current && current.agents));
	}
}
