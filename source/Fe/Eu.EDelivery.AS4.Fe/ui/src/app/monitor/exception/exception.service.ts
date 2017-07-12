import { Observable } from 'rxjs/Observable';
import { AuthHttp } from 'angular2-jwt';
import { Injectable } from '@angular/core';
import { URLSearchParams, RequestOptions } from '@angular/http';

import { Store } from './../../common/store';
import { Exception } from './../../api/Messages/Exception';
import { ExceptionStore } from './exception.store';
import { MessageResult } from './../messageresult';
import { ExceptionFilter } from './exception.filter';

@Injectable()
export class ExceptionService {
    constructor(private _http: AuthHttp, private _store: ExceptionStore) {

    }
    public getMessages(filter: ExceptionFilter) {
        let options = new RequestOptions();
        if (filter !== undefined) {
            options.search = filter.toUrlParams();
        }
        this._http
            .get(this.getUrl(), options)
            .map((result) => result.json())
            .subscribe((messages: MessageResult<Exception>) => {
                this._store.setState({
                    messages: messages.messages,
                    filter,
                    total: messages.total,
                    pages: messages.pages,
                    page: messages.page
                });
            });
    }
    public getExceptionBody(direction: number, messageId: string): Observable<string> {
        let requestOptions = new RequestOptions();
        requestOptions.search = new URLSearchParams();
        requestOptions.search.append('direction', '' + direction);
        requestOptions.search.append('messageId', messageId);
        return this._http.get('/api/monitor/exceptionbody', requestOptions).map((data) => data.text());
    }
    public reset() {
        this._store.reset();
    }
    private getUrl() {
        return `/api/monitor/exceptions`;
    }
}
