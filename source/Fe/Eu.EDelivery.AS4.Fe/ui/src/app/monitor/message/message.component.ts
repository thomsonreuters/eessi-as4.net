import { FormBuilder, FormGroup } from '@angular/forms';
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
import { timeRangeValidator } from "../../common/timeinput/timeinput.component";

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
    public filterForm: FormGroup;
    public messageFilter: MessageFilter;
    public messageDetail: api.MessageDetail;
    public advanced: boolean = false;
    @ViewChild('filter') public filter: FilterComponent;
    private _searchTrigger: Subject<boolean> = new Subject<boolean>();
    private _subscriptions: Subscription[] = new Array<Subscription>();
    constructor(private _messageStore: MessageStore, private _service: MessageService, private _formBuilder: FormBuilder) {
        this.messages = this._messageStore.changes;
        let searchTrigger = this._searchTrigger.asObservable().debounceTime(500).subscribe(() => this.filter.search());
        this._subscriptions.push(searchTrigger);
        this.filterForm = this._formBuilder.group({
            direction: [],
            ebmsMessageType: [],
            showDuplicates: [],
            showTests: [],
            status: [],
            ebmsMessageId: [],
            fromParty: [],
            toParty: [],
            mep: [],
            operation: [],
            service: [],
            actionName: [],
            mpc: [],
            insertionTimeType: [],
            insertionTimeFrom: [],
            insertionTimeTo: [],
            page: []
        }, { validator: timeRangeValidator('insertionTimeType', 'insertionTimeFrom', 'insertionTimeTo') });
        this.messageFilter = new MessageFilter(this.filterForm.value);
        const filterValueChanges = this.filterForm.valueChanges.subscribe((result) => this.messageFilter = new MessageFilter(result));
        this._subscriptions.push(filterValueChanges);
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
    }
    public toggleSearch() {
        this._searchTrigger.next(true);
    }
    public filtersLoaded(filter: MessageFilter) {
        this.filterForm.reset(filter);
        if (!this.advanced) {
            this.advanced = filter.isAdvanced();
        }
    }
}
