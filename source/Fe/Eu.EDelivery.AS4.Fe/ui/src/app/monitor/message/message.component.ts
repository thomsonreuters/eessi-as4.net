import { MESSAGESERVICETOKEN } from './../service.token';
import { ActivatedRoute, Router, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { Component, trigger, style, EventEmitter, Output, Inject } from '@angular/core';

import { DialogService } from './../../common/dialog.service';
import { MessageService } from './message.service';
import { MessageFilter } from './message.filter';
import { IMessageState, MessageStore } from './message.store';
import * as api from '../../api/Messages';

@Component({
    selector: 'as4-messages',
    templateUrl: './message.component.html',
    styleUrls: ['message.component.scss'],
    providers: [
        { provide: MESSAGESERVICETOKEN, useClass: MessageService }
    ]
})
export class MessageComponent {
    public messages: Observable<IMessageState>;
    public activeMessage: api.Message | undefined;
    public messageFilter: MessageFilter = new MessageFilter();
    public messageDetail: api.MessageDetail;
    constructor(private _messageStore: MessageStore, private _service: MessageService) {
        this.messages = this._messageStore.changes;
    }
    public toggle(message: api.Message) {
        if (this.activeMessage === message) {
            this.activeMessage = undefined;
            return;
        }
        this.activeMessage = message;
        // Load the message detail
        this._service
            .getMessageDetail(message.direction, message.ebmsMessageId)
            .subscribe((result) => this.messageDetail = result);
    }
    public switchIds() {
        let tmp = this.messageFilter.ebmsRefToMessageId;
        this.messageFilter.ebmsRefToMessageId = this.messageFilter.ebmsMessageId;
        this.messageFilter.ebmsMessageId = tmp;
    }
}
