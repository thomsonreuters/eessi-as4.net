/* tslint:disable */
import { ReplyHandlingSetting } from './ReplyHandlingSetting';
import { ReceiveReliability } from "./ReceiveReliability";
import { ReceiveErrorHandling } from "./ReceiveErrorHandling";
import { Receivehandling } from "./Receivehandling";
import { ReceiveSecurity } from "./ReceiveSecurity";
import { MessagePackaging } from "./MessagePackaging";
import { Deliver } from "./Deliver";
import { MessageHandling } from './MessageHandling';

export class ReceivingProcessingMode {
    id: string;
    reliability: ReceiveReliability = new ReceiveReliability();
    replyHandling: ReplyHandlingSetting = new ReplyHandlingSetting();
    exceptionHandling: Receivehandling = new Receivehandling();
    security: ReceiveSecurity = new ReceiveSecurity();
    messagePackaging: MessagePackaging = new MessagePackaging();
    deliver: Deliver = new Deliver();
    messageHandling: MessageHandling = new MessageHandling();

    static FIELD_id: string = 'id';
    static FIELD_reliability: string = 'reliability';
    static FIELD_replyHandling: string = 'replyHandling';
    static FIELD_exceptionHandling: string = 'exceptionHandling';
    static FIELD_security: string = 'security';
    static FIELD_messagePackaging: string = 'messagePackaging';
    static FIELD_deliver: string = 'deliver';    
    static FIELD_messageHandling: string = 'messageHandling';
}
