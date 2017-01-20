import { PMODECRUD_SERVICE } from './../crud/crud.component';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs/Subscription';
import { Observable } from 'rxjs/Observable';
import { FormGroup, FormBuilder, FormArray, FormControl, AbstractControl } from '@angular/forms';
import { Component, ViewChildren, QueryList, OnInit, Inject, OnDestroy } from '@angular/core';

import { BasePmodeComponent } from './../basepmode/basepmode.component';
import { ReceivingPmode } from './../../api/ReceivingPmode';
import { PmodesModule } from '../pmodes.module';
import { PmodeStore } from '../pmode.store';
import { DialogService } from './../../common/dialog.service';
import { ItemType } from './../../api/ItemType';
import { RuntimeStore } from './../../settings/runtime.store';
import { SendingPmode } from './../../api/SendingPmode';
import { ReceivingProcessingMode } from './../../api/ReceivingProcessingMode';
import { getRawFormValues } from './../../common/getRawFormValues';
import { ModalService } from './../../common/modal/modal.service';
import { BoxComponent } from './../../common/box/box.component';
import { ReceivingPmodeService } from './../pmode.service';

@Component({
    templateUrl: './receivingpmode.component.html',
    styles: ['../basepmode/basepmode.component.scss'],
    providers: [
        { provide: PMODECRUD_SERVICE, useClass: ReceivingPmodeService }
    ]
})
export class ReceivingPmodeComponent implements OnDestroy {
    public deliverSenders;
    private subscriptions: Subscription[] = new Array<Subscription>();
    constructor(private _runtimeStore: RuntimeStore) {
        this._runtimeStore
            .changes
            .filter((store) => !!!store)
            .map((store) => store.deliverSenders)
            .subscribe((data) => this.deliverSenders = data);
    }
    public ngOnDestroy() {
        this.subscriptions.forEach((subs) => subs.unsubscribe);
    }
}
