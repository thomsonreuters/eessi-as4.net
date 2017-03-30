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
    public hash: string;
}
