import { Subscription } from 'rxjs/Subscription';
import { ActivatedRoute, Router, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, Subject } from 'rxjs';
import { Component, trigger, style, EventEmitter, Output, Inject, ChangeDetectionStrategy, AfterViewChecked, ViewChild, OnDestroy } from '@angular/core';

import { DialogService } from './../../common/dialog.service';
import { MessageService } from './message.service';
import { MessageFilter } from './message.filter';
import { IMessageState, MessageStore } from './message.store';
import * as api from '../../api/Messages';
import { FilterComponent } from './../filter/filter.component';
import { MESSAGESERVICETOKEN } from './../service.token';

@Component({
    selector: 'as4-messages',
    templateUrl: './message.component.html',
    styleUrls: ['message.component.scss'],
    providers: [
        { provide: MESSAGESERVICETOKEN, useClass: MessageService }
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class MessageComponent implements OnDestroy {
    public messages: Observable<IMessageState>;
    public activeMessage: api.Message | undefined;
    public messageFilter: MessageFilter = new MessageFilter();
    public messageDetail: api.MessageDetail;
    @ViewChild('filter') public filter: FilterComponent;
    private _searchTrigger: Subject<boolean> = new Subject<boolean>();
    private _subscriptions: Subscription[] = new Array<Subscription>();
    constructor(private _messageStore: MessageStore, private _service: MessageService) {
        this.messages = this._messageStore.changes;
        let searchTrigger = this._searchTrigger.asObservable().debounceTime(500).subscribe(() => this.filter.search());
        this._subscriptions.push(searchTrigger);
    }
    public ngOnDestroy() {
        this._subscriptions.forEach((sub) => sub.unsubscribe());
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
    public toggleSearch() {
        this._searchTrigger.next(true);
    }
}
