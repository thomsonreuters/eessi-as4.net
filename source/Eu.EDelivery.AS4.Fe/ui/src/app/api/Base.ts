import { CertificateStore } from './CertificateStore';
import { SettingsRetryReliability } from './SettingsRetryReliability';

export class Base {
  public static FIELD_idFormat: string = 'idFormat';
  public static FIELD_retentionPeriod: string = 'retentionPeriod';
  public static FIELD_retryReliability: string = 'retryReliability';
  public static FIELD_certificateStore: string = 'certificateStore';

  public idFormat: string;
  public retentionPeriod: number;
  public retryReliability: SettingsRetryReliability;
  public certificateStore: CertificateStore;
}
