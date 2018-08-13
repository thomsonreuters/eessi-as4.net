import { Observer } from 'rxjs/Observer';
import { ExceptionFilter } from './../exception/exception.filter';
import { Observable } from 'rxjs/Observable';
import { AuthHttp } from 'angular2-jwt';
import { Injectable } from '@angular/core';
import { URLSearchParams, RequestOptions } from '@angular/http';

import { Store } from './../../common/store';
import { MessageResult } from './../messageresult';
import { MessageStore } from './message.store';
import { Exception } from '../../api/Messages/Exception';
import { MessageFilter } from './message.filter';
import { ExceptionService } from '../exception/exception.service';
import * as api from '../../api/Messages';
import { CustomAuthNoSpinnerHttp } from './../../common/spinner/customhttp';

@Injectable()
export class MessageService {
    private _baseUrl: string;
    constructor(private _http: AuthHttp, private _noSpinnerHttp: CustomAuthNoSpinnerHttp, private _store: MessageStore, private _exceptionService: ExceptionService) { }
    public getMessages(filter: MessageFilter): Observable<boolean> {
        return Observable.create((obs: Observer<boolean>) => {
            let options = new RequestOptions();
            if (filter !== undefined) {
                options.search = filter.toUrlParams();
            }
            this._http
                .get(this.getUrl(), options)
                .map((result) => result.json())
                .subscribe((messages: MessageResult<api.Message>) => {
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
    public getExceptions(direction: number, message: api.Message): Observable<boolean> {
        if (!!!message.ebmsMessageId) {
            this._exceptionService.reset();
            return Observable.of(false);
        }

        let filter = new ExceptionFilter();
        filter.insertionTimeType = -1;
        filter.ebmsRefToMessageId = message.ebmsMessageId;
        return this._exceptionService.getMessages(filter, true);
    }
    public getRelatedMessages(direction: number, messageId: string): Observable<boolean> {
        return Observable.create((obs: Observer<boolean>) => {
            let urlParams = new URLSearchParams();
            urlParams.append('direction', '' + direction);
            urlParams.append('messageId', messageId);
            let options = new RequestOptions();
            options.search = urlParams;
            this._noSpinnerHttp
                .get(`/api/monitor/relatedmessages`, options)
                .map((result) => result.json())
                .subscribe((messages: MessageResult<api.Message>) => {
                    this._store.update('relatedMessages', messages.messages);
                    obs.next(true);
                    obs.complete();
                }, () => {
                    obs.next(false);
                    obs.complete();
                });
        });
    }
    public getMessageBody(direction: number, messageId: string): Observable<string> {
        let requestOptions = new RequestOptions();
        requestOptions.search = new URLSearchParams();
        requestOptions.search.append('direction', '' + direction);
        requestOptions.search.append('id', messageId);
        return this._http.get('/api/monitor/messagebody', requestOptions).map((data) => data.text());
    }
    private getUrl(action: string = 'messages'): string {
        return `/api/monitor/` + (!!!action ? '' : action);
    }
}
