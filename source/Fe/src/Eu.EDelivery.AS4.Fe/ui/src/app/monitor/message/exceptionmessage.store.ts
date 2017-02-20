import { Injectable } from '@angular/core';

import { Exception } from './../../api/Messages/Exception';
import { Store } from './../../common/store';
import { Message } from './../../api/Messages/Message';

export interface IExceptionMessageStoreState {
    exceptions: Exception[];
    currentMessage: Message;
    total: number;
}

@Injectable()
export class ExceptionMessageStore extends Store<IExceptionMessageStoreState> {
    constructor() {
        super({
            exceptions: new Array<Exception>(),
            currentMessage: null,
            total: 0
        });
    }
}
