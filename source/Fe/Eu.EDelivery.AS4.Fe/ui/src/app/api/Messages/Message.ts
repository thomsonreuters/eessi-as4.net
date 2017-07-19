export class Message {
    public operation: string;
    public operationMethod: string;
    public contentType: string;
    public ebmsMessageId: string;
    public ebmsRefToMessageId: string;
    public insertionTime: Date;
    public modificationTime: Date;
    public exceptionType: string;
    public status: string;
    public hasExceptions: boolean;
    public fromParty: string;
    public toParty: string;
    public action: string;
    public conversationId: string;
    public isDuplicate: boolean;
    public isTest: boolean;
    public hash: string;
    public direction: number;
    public mep: string;
}
