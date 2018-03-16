import { Component, OnDestroy, ViewChild } from '@angular/core';
import { FormGroupDirective } from '@angular/forms';
import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs/Subscription';

import { ReceivingPmodeService } from '../receivingpmode.service';
import { ItemType } from './../../api/ItemType';
import { CanComponentDeactivate } from './../../common/candeactivate.guard';
import { RuntimeStore } from './../../settings/runtime.store';
import { PMODECRUD_SERVICE } from './../crud/crud.component';

@Component({
    templateUrl: './receivingpmode.component.html',
    styles: ['../basepmode/basepmode.component.scss'],
    providers: [
        { provide: PMODECRUD_SERVICE, useClass: ReceivingPmodeService }
    ]
})
export class ReceivingPmodeComponent implements OnDestroy, CanComponentDeactivate {
    public deliverSenders$: Observable<ItemType[]>;
    public attachmentUploaders$: Observable<ItemType[]>;
    private subscriptions: Subscription[] = new Array<Subscription>();
    @ViewChild(FormGroupDirective) private formGroup: FormGroupDirective;
    constructor(private _runtimeStore: RuntimeStore) {
        this.deliverSenders$ = this
            ._runtimeStore.changes.filter((store) => !!store).map((store) => store.deliverSenders);
        this.attachmentUploaders$ = this
            ._runtimeStore.changes.filter((store) => !!store).map((store) => store.attachmentUploaders);
    }
    public ngOnDestroy() {
        this.subscriptions.forEach((subs) => subs.unsubscribe);
    }
    public canDeactivate(): boolean {
        return !this.formGroup.dirty;
    }
}
