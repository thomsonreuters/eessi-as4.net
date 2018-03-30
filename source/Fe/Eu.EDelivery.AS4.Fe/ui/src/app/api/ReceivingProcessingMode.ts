/* tslint:disable */
import { MessageHandling } from './MessageHandling';
import { MessagePackaging } from './MessagePackaging';
import { Receivehandling } from './Receivehandling';
import { ReceiveReliability } from './ReceiveReliability';
import { ReceiveSecurity } from './ReceiveSecurity';
import { ReplyHandlingSetting } from './ReplyHandlingSetting';

export class ReceivingProcessingMode {
    id: string;
    reliability: ReceiveReliability = new ReceiveReliability();
    replyHandling: ReplyHandlingSetting = new ReplyHandlingSetting();
    exceptionHandling: Receivehandling = new Receivehandling();
    security: ReceiveSecurity = new ReceiveSecurity();
    messagePackaging: MessagePackaging = new MessagePackaging();
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
