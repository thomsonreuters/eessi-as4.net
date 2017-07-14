/* tslint:disable */
import { ReplyHandlingSetting } from './ReplyHandlingSetting';
import { ReceiveReliability } from "./ReceiveReliability";
import { ReceiveErrorHandling } from "./ReceiveErrorHandling";
import { Receivehandling } from "./Receivehandling";
import { ReceiveSecurity } from "./ReceiveSecurity";
import { MessagePackaging } from "./MessagePackaging";
import { Deliver } from "./Deliver";

export class ReceivingProcessingMode {
    id: string;
    mep: number;
    mepBinding: number;
    reliability: ReceiveReliability = new ReceiveReliability();
    replyHandling: ReplyHandlingSetting = new ReplyHandlingSetting();
    exceptionHandling: Receivehandling = new Receivehandling();
    security: ReceiveSecurity = new ReceiveSecurity();
    messagePackaging: MessagePackaging = new MessagePackaging();
    deliver: Deliver = new Deliver();

    static FIELD_id: string = 'id';
    static FIELD_mep: string = 'mep';
    static FIELD_mepBinding: string = 'mepBinding';
    static FIELD_reliability: string = 'reliability';
    static FIELD_replyHandling: string = 'replyHandling';
    static FIELD_exceptionHandling: string = 'exceptionHandling';
    static FIELD_security: string = 'security';
    static FIELD_messagePackaging: string = 'messagePackaging';
    static FIELD_deliver: string = 'deliver';    
}
