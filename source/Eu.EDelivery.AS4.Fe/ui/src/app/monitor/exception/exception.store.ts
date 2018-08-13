import { Injectable } from '@angular/core';

import { Exception } from './../../api/Messages/Exception';
import { Store } from './../../common/store';
import { ExceptionFilter } from './exception.filter';

export interface IExceptionState {
    messages: Exception[];
    filter: ExceptionFilter | null;
    total: number;
    pages: number;
    page: number;
}

@Injectable()
export class ExceptionStore extends Store<IExceptionState> {
    constructor() {
        super({
            messages: new Array<Exception>(),
            filter: null,
            total: 0,
            pages: 0,
            page: 0
        });
    }
    public reset() {
        this.setState({
            messages: new Array<Exception>(),
            filter: null,
            total: 0,
            pages: 0,
            page: 0
        });
    }
}
