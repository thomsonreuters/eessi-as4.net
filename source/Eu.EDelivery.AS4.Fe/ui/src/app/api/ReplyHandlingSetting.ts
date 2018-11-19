/* tslint:disable */
import { ReceiveErrorHandling } from './ReceiveErrorHandling';
import { ReceiveReceiptHandling } from './ReceiveReceiptHandling';
import { RetryReliability } from './RetryReliability';

export class ReplyHandlingSetting {
  replyPattern: number;
  receiptHandling: ReceiveReceiptHandling;
  errorHandling: ReceiveErrorHandling;
  piggyBackReliability: RetryReliability;

  public static FIELD_replyPattern = 'replyPattern';
  public static FIELD_receiptHandling = 'receiptHandling';
  public static FIELD_receiveErrorHandling = 'errorHandling';
  public static FIELD_piggyBackReliability = 'piggyBackReliability';
}
