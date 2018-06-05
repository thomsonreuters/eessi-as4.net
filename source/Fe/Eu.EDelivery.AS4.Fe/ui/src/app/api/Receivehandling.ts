import { Method } from './Method';
import { RetryReliability } from './RetryReliability';

/* tslint:disable */
export class Receivehandling {
  notifyMessageConsumer: boolean;
  notifyMethod: Method;
  reliability: RetryReliability;

  static FIELD_notifyMessageConsumer: string = 'notifyMessageConsumer';
  static FIELD_notifyMethod: string = 'notifyMethod';
  static FIELD_reliability: string = 'reliability';
}
