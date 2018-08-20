import { BaseFilter } from './../base.filter';

export class MessageFilter extends BaseFilter {
    public ebmsMessageId: string;
    public ebmsRefToMessageId: string;
    public contentType: string;
    public operation: string;
    public modificationTimeFrom: Date;
    public modificationTimeTo: Date;
    public mep: string;
    public ebmsMessageType: string;
    public exceptionType: string;
    public fromParty: string;
    public toparty: string;
    public showTestMessages: boolean;
    public showDuplicates: boolean;
    public actionName: string;
    public service: string;
    public mpc: string;
    constructor(init?: Partial<MessageFilter>) {
        super();
        if (!!init) {
            Object.assign(this, init);
        }
        this.setDefaults();
    }
    public isAdvanced(): boolean {
        return !!this.mep || !!this.operation || !!this.service || !!this.actionName || !!this.mpc;
    }
}
