import { BaseFilter } from './../base.filter';

export class MessageFilter extends BaseFilter {
    public direction: number = 0;
    public ebmsMessageId: string;
    public ebmsRefToMessageId: string;
    public contentType: string;
    public operation: string;
    public insertionTimeFrom: Date;
    public insertionTimeTo: Date;
    public modificationTimeFrom: Date;
    public modificationTimeTo: Date;
    public mep: string;
    public ebmsMessageType: string;
    public exceptionType: string;
}
