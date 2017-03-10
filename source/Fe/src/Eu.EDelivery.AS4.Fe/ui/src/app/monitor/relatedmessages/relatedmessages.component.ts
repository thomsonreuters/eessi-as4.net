import { Observable } from 'rxjs/Observable';
import { MessageFilter } from './../message/message.filter';
import { Component, Inject, Input, OnInit, SkipSelf } from '@angular/core';
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
    public messages: Observable<IMessageState>;
    constructor( @Inject(MESSAGESERVICETOKEN) private _messageService: MessageService, private _messageStore: MessageStore) {
        this.messages = this._messageStore.changes;
    }
    public ngOnInit() {
        if (!!!this.messageId) {
            return;
        }

        this._messageService.getRelatedMessages(this.direction, this.messageId);
    }
}
