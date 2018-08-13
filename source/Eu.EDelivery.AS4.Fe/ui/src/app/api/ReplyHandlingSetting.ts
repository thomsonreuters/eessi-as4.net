/* tslint:disable */
import { ReceiveReceiptHandling } from './ReceiveReceiptHandling';
import { ReceiveErrorHandling } from './ReceiveErrorHandling';

export class ReplyHandlingSetting {
    replyPattern: number;
    sendingPMode: string;
    receiptHandling: ReceiveReceiptHandling;
    errorHandling: ReceiveErrorHandling;

    public static FIELD_replyPattern = 'replyPattern';
    public static FIELD_sendingPmode = 'sendingPMode';
    public static FIELD_receiptHandling = 'receiptHandling';
    public static FIELD_receiveErrorHandling = 'errorHandling';
}
