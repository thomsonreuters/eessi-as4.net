/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

import { SettingsDatabase } from "./SettingsDatabase";
import { CustomSettings } from "./CustomSettings";
import { SettingsAgents } from "./SettingsAgents";
import { Repository } from './Repository';
import { CertificateStore } from './CertificateStore';

export class Settings {
	idFormat: string;
	certificateStoreName: string;
	database: SettingsDatabase;
	customSettings: CustomSettings;
	agents: SettingsAgents;
	certificateStore: CertificateStore;

	static FIELD_idFormat: string = 'idFormat';
	static FIELD_certificateStoreName: string = 'certificateStoreName';
	static FIELD_database: string = 'database';
	static FIELD_customSettings: string = 'customSettings';
	static FIELD_agents: string = 'agents';

	static getForm(formBuilder: FormBuilder, current: Settings): FormGroup {
		return formBuilder.group({
			idFormat: [current && current.idFormat],
			certificateStoreName: [current && current.certificateStoreName],
			database: SettingsDatabase.getForm(formBuilder, current && current.database),
			customSettings: CustomSettings.getForm(formBuilder, current && current.customSettings),
			agents: SettingsAgents.getForm(formBuilder, current && current.agents),
		});
	}
}
