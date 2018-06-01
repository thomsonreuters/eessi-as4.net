export class RetryReliability {
  isEnabled: boolean;
  retryCount: number;
  retryInterval: string;

  static FIELD_isEnabled: string = 'isEnabled';
  static FIELD_retryCount: string = 'retryCount';
  static FIELD_retryInterval: string = 'retryInterval';
}
