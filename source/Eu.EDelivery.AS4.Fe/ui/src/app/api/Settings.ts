import { CertificateStore } from './CertificateStore';
import { CustomSettings } from './CustomSettings';
import { SettingsAgents } from './SettingsAgents';
import { SettingsDatabase } from './SettingsDatabase';
import { SettingsPullSend } from './SettingsPullSend';
import { SettingsSubmit } from './SettingsSubmit';

/* tslint:disable */
export class Settings {
  idFormat: string;
  retentionPeriod: number;
  submit: SettingsSubmit;
  pullSend: SettingsPullSend;
  database: SettingsDatabase;
  certificateStore: CertificateStore;
  customSettings: CustomSettings;
  agents = new SettingsAgents();
}
