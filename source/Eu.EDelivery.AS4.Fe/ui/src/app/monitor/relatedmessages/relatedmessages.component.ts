import { Observable } from 'rxjs/Observable';
import { Component, Inject, Input, OnInit, SkipSelf } from '@angular/core';

import { MessageFilter } from './../message/message.filter';
import { Message } from './../../api/Messages/Message';
import { MessageStore, IMessageState } from './../message/message.store';
import { MESSAGESERVICETOKEN } from './../service.token';
import { MessageService } from './../message/message.service';

@Component({
    selector: 'as4-related-messages',
    templateUrl: 'relatedmessages.component.html',
    providers: [
        { provide: MESSAGESERVICETOKEN, useClass: MessageService },
        { provide: MessageStore, useClass: MessageStore }
    ]
})
export class RelatedMessagesComponent implements OnInit {
    @Input() public messageId: string;
    @Input() public direction: number;
    public messages: Observable<Message[] | undefined>;
    public isLoading: boolean = false;
    constructor( @Inject(MESSAGESERVICETOKEN) private _messageService: MessageService, private _messageStore: MessageStore) {
        this.messages = this._messageStore.changes.map((store) => store.relatedMessages);
    }
    public ngOnInit() {
        if (!!!this.messageId) {
            return;
        }

        this.isLoading = true;
        this._messageService
            .getRelatedMessages(this.direction, this.messageId)
            .subscribe(() => this.isLoading = false);
    }
}
