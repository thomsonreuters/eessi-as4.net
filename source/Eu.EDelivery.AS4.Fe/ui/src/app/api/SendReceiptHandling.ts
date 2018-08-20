import { Method } from './Method';
import { RetryReliability } from './RetryReliability';

/* tslint:disable */
export class SendReceiptHandling {
  verifyNRR: boolean;
  notifyMessageProducer: boolean;
  notifyMethod: Method;
  reliability: RetryReliability;

  static FIELD_notifyMessageProducer: string = 'notifyMessageProducer';
  static FIELD_notifyMethod: string = 'notifyMethod';
  static FIELD_verifyNRR: string = 'verifyNRR';
  static FIELD_reliability: string = 'reliability';
}
