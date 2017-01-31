import { Injectable } from '@angular/core';

import { InException } from './../../api/Messages/InException';
import { Store } from './../../common/store';
import { InExceptionFilter } from './inexception.filter';

export interface IInExceptionState {
    messages: InException[];
    filter: InExceptionFilter;
    total: number;
    pages: number;
    page: number;
}

@Injectable()
export class InExceptionStore extends Store<IInExceptionState> {
    constructor() {
        super({
            messages: new Array<InException>(),
            filter: null,
            total: 0,
            pages: 0,
            page: 0
        });
    }
}
