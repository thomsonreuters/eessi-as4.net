import { CertificateStore } from './CertificateStore';
import { CustomSettings } from './CustomSettings';
import { SettingsAgents } from './SettingsAgents';
import { SettingsDatabase } from './SettingsDatabase';
import { SettingsPullSend } from './SettingsPullSend';

/* tslint:disable */
export class Settings {
  idFormat: string;
  retentionPeriod: number;
  pullSend: SettingsPullSend;
  database: SettingsDatabase;
  certificateStore: CertificateStore;
  customSettings: CustomSettings;
  agents = new SettingsAgents();

  static FIELD_idFormat: string = 'idFormat';
  static FIELD_retentionPeriod: string = 'retentionPeriod';
  static FIELD_pullSend: string = 'pullSend';
  static FIELD_database: string = 'database';
  static FIELD_certificateStore: string = 'certificateStore';
  static FIELD_customSettings: string = 'customSettings';
  static FIELD_agents: string = 'agents';
}
