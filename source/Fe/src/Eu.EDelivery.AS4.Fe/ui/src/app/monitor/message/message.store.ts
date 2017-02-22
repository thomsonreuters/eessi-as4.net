import { MessageFilter } from './message.filter';
import { Message } from './../../api/Messages/Message';
import { Store } from '../../common/store';

    export interface IMessageState {
    messages?: Message[];
    total?: number;
    pages?: number;
    page?: number;
    filter?: MessageFilter;
}

export class MessageStore extends Store<IMessageState> {
    constructor() {
        super({
            messages: new Array<Message>(),
            filter: new MessageFilter()
        });
    }
}
