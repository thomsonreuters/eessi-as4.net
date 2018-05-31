/* tslint:disable */
import { DynamicDiscovery } from './DynamicDiscovery';
import { PushConfiguration } from './PushConfiguration';
import { Security } from './Security';
import { SendHandling } from './SendHandling';
import { SendMessagePackaging } from './SendMessagePackaging';
import { SendReceiptHandling } from './SendReceiptHandling';
import { SendReliability } from './SendReliability';

export class SendingProcessingMode {
  id: string;
  allowOverride: boolean;
  mep: number;
  mepBinding: number;
  pushConfiguration: PushConfiguration = new PushConfiguration();
  dynamicDiscovery: DynamicDiscovery = new DynamicDiscovery();
  reliability: SendReliability = new SendReliability();
  receiptHandling: SendReceiptHandling = new SendReceiptHandling();
  errorHandling: SendHandling = new SendHandling();
  exceptionHandling: SendHandling = new SendHandling();
  security: Security = new Security();
  messagePackaging: SendMessagePackaging = new SendMessagePackaging();

  static FIELD_id: string = 'id';
  static FIELD_allowOverride: string = 'allowOverride';
  static FIELD_mep: string = 'mep';
  static FIELD_mepBinding: string = 'mepBinding';
  static FIELD_pushConfiguration: string = 'pushConfiguration';
  static FIELD_dynamicDiscovery: string = 'dynamicDiscovery';
  static FIELD_reliability: string = 'reliability';
  static FIELD_receiptHandling: string = 'receiptHandling';
  static FIELD_errorHandling: string = 'errorHandling';
  static FIELD_exceptionHandling: string = 'exceptionHandling';
  static FIELD_security: string = 'security';
  static FIELD_messagePackaging: string = 'messagePackaging';
}
