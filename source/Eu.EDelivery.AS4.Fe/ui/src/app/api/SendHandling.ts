import { Method } from './Method';
import { RetryReliability } from './RetryReliability';

export class SendHandling {
  notifyMessageProducer: boolean;
  notifyMethod: Method;
  reliability: RetryReliability;

  static FIELD_notifyMessageProducer: string = 'notifyMessageProducer';
  static FIELD_notifyMethod: string = 'notifyMethod';
  static FIELD_reliability: string = 'reliability';
}
