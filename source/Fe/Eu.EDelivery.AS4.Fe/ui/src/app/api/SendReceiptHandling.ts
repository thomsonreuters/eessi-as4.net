import { SendHandling } from "./SendHandling";
import { Method } from "./Method";

/* tslint:disable */
export class SendReceiptHandling {
    verifyNRR: boolean;
    notifyMessageProducer: boolean;
	notifyMethod: Method;

	static FIELD_notifyMessageProducer: string = 'notifyMessageProducer';
    static FIELD_notifyMethod: string = 'notifyMethod';
    public static FIELD_verifyNRR = 'verifyNRR';

}