import { SettingsDatabase } from "./SettingsDatabase"
import { CustomSettings } from "./CustomSettings"
import { SettingsAgents } from "./SettingsAgents"

export class Settings {
		idFormat: string;
		certificateStoreName: string;

		database: SettingsDatabase;
		customSettings: CustomSettings;
		agents: SettingsAgents;
}