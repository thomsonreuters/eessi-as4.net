import { Observable } from 'rxjs/Observable';
import { AuthHttp } from 'angular2-jwt';
import { Injectable } from '@angular/core';
import { URLSearchParams, RequestOptions } from '@angular/http';

import { Store } from './../../common/store';
import { InException } from './../../api/Messages/InException';
import { InExceptionStore } from './inexception.store';
import { InExceptionFilter } from './inexception.filter';
import { MessageResult } from './../messageresult';

@Injectable()
export class InExceptionService {
    private _baseUrl: string = '/api/monitor/inexception';
    constructor(private _http: AuthHttp, private _store: InExceptionStore) {

    }
    public getMessages(filter?: InExceptionFilter) {
        let options = new RequestOptions();
        if (filter !== undefined) {
            options.search = filter.toUrlParams();
        }
        this._http
            .get(this.getUrl(), options)
            .map((result) => result.json())
            .subscribe((messages: MessageResult<InException>) => {
                this._store.setState({
                    messages: messages.messages,
                    filter,
                    total: messages.total,
                    pages: messages.pages,
                    page: messages.page
                });
            });
    }
    private getUrl(action?: string): string {
        if (!!!action) {
            return this._baseUrl;
        }
        return `${this._baseUrl}/${action}`;
    }
}

