import { Subscription } from 'rxjs/Subscription';
import { FormBuilder, FormGroupDirective, FormGroup } from '@angular/forms';
import { Component, OnDestroy, ViewChild } from '@angular/core';
import { Observable } from 'rxjs/Observable';

import { RuntimeStore } from './../../settings/runtime.store';
import { PMODECRUD_SERVICE } from './../crud/crud.component';
import { SendingPmodeService } from './../sendingpmode.service';
import { SendingProcessingMode } from './../../api/SendingProcessingMode';
import { BasePmodeComponent } from './../basepmode/basepmode.component';
import { SendingPmode } from './../../api/SendingPmode';
import { ReceivingPmode } from './../../api/ReceivingPmode';
import { ItemType } from './../../api/ItemType';
import { CanComponentDeactivate } from './../../common/candeactivate.guard';

@Component({
    selector: 'as4-sending-pmode',
    templateUrl: './sendingpmode.component.html',
    styles: ['../basepmode/basepmode.component.scss'],
    providers: [
        { provide: PMODECRUD_SERVICE, useClass: SendingPmodeService }
    ]
})
export class SendingPmodeComponent implements OnDestroy, CanComponentDeactivate {
    public mask: any[] = [/[0-9]/, /[0-9]/, ':', /[0-5]/, /[0-9]/, ':', /[0-5]/, /[0-9]/];
    public regexUrl: RegExp = /^(https?:\/\/)[\w:\/]+/; 
    public notifySenders$: Observable<ItemType[]>;
    public dynamicdiscoveryprofiles$: Observable<ItemType[]>;
    @ViewChild(FormGroupDirective) private formGroup: FormGroupDirective;
    private subscriptions: Subscription[] = new Array<Subscription>();
    constructor(private _runtimeStore: RuntimeStore) {
        this.notifySenders$ = this
            ._runtimeStore.changes.filter((store) => !!store).map((store) => store.notifySenders);
        this.dynamicdiscoveryprofiles$ = this
            ._runtimeStore.changes.filter((store) => !!store).map((store) => store.dynamicDiscoveryProfiles);
    }
    public ngOnDestroy() {
        this.subscriptions.forEach((subs) => subs.unsubscribe);
    }
    public canDeactivate(): boolean {
        return !this.formGroup.dirty;
    }
}
