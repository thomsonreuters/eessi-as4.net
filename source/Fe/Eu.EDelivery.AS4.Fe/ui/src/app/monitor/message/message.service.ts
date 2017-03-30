import { ExceptionFilter } from './../exception/exception.filter';
import { Observable } from 'rxjs/Observable';
import { AuthHttp } from 'angular2-jwt';
import { Injectable } from '@angular/core';
import { URLSearchParams, RequestOptions } from '@angular/http';

import { Store } from './../../common/store';
import { MessageResult } from './../messageresult';
import { MessageStore } from './message.store';
import { Message } from '../../api/Messages/Message';
import { Exception } from '../../api/Messages/Exception';
import { MessageFilter } from './message.filter';
import { ExceptionService } from '../exception/exception.service';

@Injectable()
export class MessageService {
    private _baseUrl: string;
    constructor(private _http: AuthHttp, private _store: MessageStore, private _exceptionService: ExceptionService) {
    }
    public getMessages(filter: MessageFilter) {
        let options = new RequestOptions();
        if (filter !== undefined) {
            options.search = filter.toUrlParams();
        }
        this._http
            .get(this.getUrl(), options)
            .map((result) => result.json())
            .subscribe((messages: MessageResult<Message>) => {
                this._store.setState({
                    messages: messages.messages,
                    filter,
                    total: messages.total,
                    pages: messages.pages,
                    page: messages.page
                });
            });
    }
    public getExceptions(direction: number, message: Message) {
        if (!!!message.ebmsRefToMessageId) {
            this._exceptionService.reset();
            return;
        }

        let filter = new ExceptionFilter();
        filter.ebmsRefToMessageId = message.ebmsRefToMessageId;
        this._exceptionService.getMessages(filter);
    }
    public getRelatedMessages(direction: number, messageId: string) {
        let urlParams = new URLSearchParams();
        urlParams.append('direction', '' + direction);
        urlParams.append('messageId', messageId);
        let options = new RequestOptions();
        options.search = urlParams;
        this._http
            .get(`/api/monitor/relatedmessages`, options)
            .map((result) => result.json())
            .subscribe((messages: MessageResult<Message>) => {
                this._store.update('relatedMessages', messages.messages);
            });
    }
    public getMessageBody(direction: number, messageId: string): Observable<string> {
        let requestOptions = new RequestOptions();
        requestOptions.search = new URLSearchParams();
        requestOptions.search.append('direction', '' + direction);
        requestOptions.search.append('messageId', messageId);
        return this._http.get('/api/monitor/messagebody', requestOptions).map((data) => data.text());
    }
    private getUrl(): string {
        return `/api/monitor/messages/`;
    }
}
