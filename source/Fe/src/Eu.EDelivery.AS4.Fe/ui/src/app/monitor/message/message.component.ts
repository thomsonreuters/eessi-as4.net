import { ActivatedRoute, Router, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { Component, trigger, style, EventEmitter, Output, Inject } from '@angular/core';

import { Message } from './../../api/Messages/Message';
import { DialogService } from './../../common/dialog.service';
import { MessageService } from './message.service';
import { MessageFilter } from './message.filter';
import { IMessageState, MessageStore } from './message.store';

@Component({
    selector: 'as4-messages',
    templateUrl: './message.component.html',
    styleUrls: ['message.component.scss']
})
export class MessageComponent {
    public messages: Observable<IMessageState>;
    public activeMessage: Message | undefined;
    public messageFilter: MessageFilter = new MessageFilter();
    constructor(private _messageStore: MessageStore, private _service: MessageService) {
        this.messages = this._messageStore.changes;
    }
    public toggle(message: Message) {
        if (this.activeMessage === message) {
            this.activeMessage = undefined;
            return;
        }
        this.activeMessage = message;
    }
    public directionChanged() {
        if (this.messageFilter.direction === 0) {
            this._service.setInMode();
        } else {
            this._service.setOutMode();
        }
    }
}
