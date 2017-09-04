/* tslint:disable */

import { Method } from './Method';

export class MessageHandling {
    public messageHandlingType: number;
    public item: Deliver | Forward | null;

    public static FIELD_messageHandlingType: string = 'messageHandlingType';
    public static FIELD_item: string = 'item';
}

export class Deliver {
    public isEnabled: boolean;
    public payloadReferenceMethod: Method;
    public deliverMethod: Method;

    public static FIELD_isEnabled: string = 'isEnabled';
    public static FIELD_payloadReferenceMethod: string = 'payloadReferenceMethod';
    public static FIELD_deliverMethod: string = 'deliverMethod';
}

export class Forward {
    public sendingPMode: string;

    public static FIELD_sendingPmode: string = 'sendingPMode';
}
