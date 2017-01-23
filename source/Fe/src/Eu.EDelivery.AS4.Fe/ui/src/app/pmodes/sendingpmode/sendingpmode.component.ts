import { Subscription } from 'rxjs/Subscription';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Component, OnDestroy } from '@angular/core';
import { Observable } from 'rxjs/Observable';

import { RuntimeStore } from './../../settings/runtime.store';
import { PMODECRUD_SERVICE } from './../crud/crud.component';
import { SendingPmodeService } from './../sendingpmode.service';
import { SendingProcessingMode } from './../../api/SendingProcessingMode';
import { BasePmodeComponent } from './../basepmode/basepmode.component';
import { SendingPmode } from './../../api/SendingPmode';
import { ReceivingPmode } from './../../api/ReceivingPmode';

@Component({
    selector: 'as4-sending-pmode',
    templateUrl: './sendingpmode.component.html',
    styles: ['../basepmode/basepmode.component.scss'],
    providers: [
        { provide: PMODECRUD_SERVICE, useClass: SendingPmodeService }
    ]
})
export class SendingPmodeComponent implements OnDestroy {
    public mask: any[] = [/[0-9]/, /[0-9]/, ':', /[0-5]/, /[0-9]/, ':', /[0-5]/, /[0-9]/];
    public deliverSenders;
    private subscriptions: Subscription[] = new Array<Subscription>();
    constructor(private _runtimeStore: RuntimeStore) {
        this._runtimeStore
            .changes
            .filter((store) => !!store)
            .map((store) => store.deliverSenders)
            .subscribe((data) => this.deliverSenders = data);
    }
    public ngOnDestroy() {
        this.subscriptions.forEach((subs) => subs.unsubscribe);
    }
}
