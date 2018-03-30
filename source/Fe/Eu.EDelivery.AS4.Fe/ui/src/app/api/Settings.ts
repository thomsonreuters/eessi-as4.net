/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { SettingsDatabase } from "./SettingsDatabase";
import { CertificateStore } from "./CertificateStore";
import { CustomSettings } from "./CustomSettings";
import { SettingsAgents } from "./SettingsAgents";

export class Settings {
	idFormat: string;
	retentionPeriod: number;
	database: SettingsDatabase;
	certificateStore: CertificateStore;
	customSettings: CustomSettings;
	agents = new SettingsAgents();

	static FIELD_idFormat: string = 'idFormat';	
	static FIELD_retentionPeriod: string = 'retentionPeriod';
	static FIELD_database: string = 'database';
	static FIELD_certificateStore: string = 'certificateStore';
	static FIELD_customSettings: string = 'customSettings';
	static FIELD_agents: string = 'agents';	
}
