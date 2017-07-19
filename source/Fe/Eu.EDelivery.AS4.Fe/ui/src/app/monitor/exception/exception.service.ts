import { Observer } from 'rxjs/Observer';
import { Observable } from 'rxjs/Observable';
import { AuthHttp } from 'angular2-jwt';
import { Injectable } from '@angular/core';
import { URLSearchParams, RequestOptions } from '@angular/http';

import { Store } from './../../common/store';
import { Exception } from './../../api/Messages/Exception';
import { ExceptionStore } from './exception.store';
import { MessageResult } from './../messageresult';
import { ExceptionFilter } from './exception.filter';
import { CustomAuthNoSpinnerHttp } from './../../common/spinner/customhttp';

@Injectable()
export class ExceptionService {
    constructor(private _http: AuthHttp, private _noSpinnerHttp: CustomAuthNoSpinnerHttp, private _store: ExceptionStore) { }
    public getMessages(filter: ExceptionFilter, noSpinner: boolean = false): Observable<boolean> {
        return Observable.create((obs: Observer<boolean>) => {
            let options = new RequestOptions();
            if (filter !== undefined) {
                options.search = filter.toUrlParams();
            }
            let http: AuthHttp | null = null;
            if (!noSpinner) {
                http = this._http;
            } else {
                http = this._noSpinnerHttp;
            }

            http.get(this.getUrl(), options)
                .map((result) => result.json())
                .subscribe((messages: MessageResult<Exception>) => {
                    this._store.setState({
                        messages: messages.messages,
                        filter,
                        total: messages.total,
                        pages: messages.pages,
                        page: messages.page
                    });
                    obs.next(true);
                    obs.complete();
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
