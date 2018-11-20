import { PushConfiguration } from './PushConfiguration';
import { ReceiveErrorHandling } from './ReceiveErrorHandling';
import { ReceiveReceiptHandling } from './ReceiveReceiptHandling';
import { RetryReliability } from './RetryReliability';
import { Signing } from './Signing';

/* tslint:disable */
export class ReplyHandlingSetting {
  replyPattern: number;
  receiptHandling: ReceiveReceiptHandling;
  errorHandling: ReceiveErrorHandling;
  piggyBackReliability: RetryReliability;
  responseConfiguration: PushConfiguration;
  responseSigning: Signing;

  public static FIELD_replyPattern = 'replyPattern';
  public static FIELD_receiptHandling = 'receiptHandling';
  public static FIELD_receiveErrorHandling = 'errorHandling';
  public static FIELD_piggyBackReliability = 'piggyBackReliability';
  public static FIELD_responseConfiguration = 'responseConfiguration';
  public static FIELD_responseSigning = 'responseSigning';
}
