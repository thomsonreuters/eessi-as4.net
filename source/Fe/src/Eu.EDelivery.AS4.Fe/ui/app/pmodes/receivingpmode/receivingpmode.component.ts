import { BoxComponent } from './../../common/box/box.component';
import { Subscription } from 'rxjs/Subscription';
import { Observable } from 'rxjs/Observable';
import { FormGroup, FormBuilder, FormArray, FormControl, AbstractControl } from '@angular/forms';
import { Component, ViewChildren, QueryList, OnInit } from '@angular/core';

import { BasePmodeComponent } from './../basepmode/basepmode.component';
import { ReceivingPmode } from './../../api/ReceivingPmode';
import { PmodesModule } from '../pmodes.module';
import { PmodeStore } from '../pmode.store';
import { PmodeService, pmodeService } from '../pmode.service';
import { DialogService } from './../../common/dialog.service';
import { ItemType } from './../../api/ItemType';
import { RuntimeStore } from './../../settings/runtime.store';
import { SendingPmode } from './../../api/SendingPmode';
import { ReceivingProcessingMode } from './../../api/ReceivingProcessingMode';
import { getRawFormValues } from './../../common/getRawFormValues';

@Component({
    templateUrl: './receivingpmode.component.html'
})
export class ReceivingPmodeComponent extends BasePmodeComponent<ReceivingPmode> {
    patchForm(formBuilder: FormBuilder, form: FormGroup, pmode: ReceivingPmode) {
        ReceivingPmode.patchForm(this.formBuilder, this.form, this.currentPmode);
    }
    newPmode(newName: string): ReceivingPmode {
        let newPmode = new ReceivingPmode();
        newPmode.name = newName;
        newPmode.pmode = new ReceivingProcessingMode();
        newPmode.pmode.id = newName;
        newPmode.type = 0;
        return newPmode;
    }
    init() {
        this.form = ReceivingPmode.getForm(this.formBuilder, null);
        setTimeout(() => this.form.disable());
        this._runtimeStoreSubscription = this.runtimeStore
            .changes
            .filter(result => !!result)
            .map(result => result.deliverSenders)
            .distinctUntilChanged()
            .subscribe(result => this.deliverSenders = result);
        this._storeSubscription = this.pmodeStore
            .changes
            .filter(result => !!result)
            .map(result => result.ReceivingNames)
            .distinctUntilChanged()
            .subscribe(result => this.pmodes = result);
        this._currentPmodeSubscription = this.pmodeStore
            .changes
            .filter(result => !!result)
            .map(result => result.Receiving)
            .distinctUntilChanged()
            .subscribe(result => {
                this.currentPmode = result;
                ReceivingPmode.patchForm(this.formBuilder, this.form, result);
            });
        this.pmodeService.getAllReceiving();
    }
    getPmode(pmode: string) {
        this.pmodeService.setReceiving(pmode);
    }
    setPmode(pmode: string | undefined) {
        this.pmodeStore.setReceiving(<any>pmode);
    }
    createPmode(value: any): Observable<boolean> {
        return this.pmodeService.createReceiving(value);
    }
    updatePmode(value: any, originalName: string): Observable<boolean> {
        return this.pmodeService.updateReceiving(value, originalName);
    }
    deletePmode(value: any) {
        this.pmodeService.deleteReceiving(value);
    }
    getByName(name: string): Observable<ReceivingPmode> {
        return this.pmodeService.getReceivingByName(name);
    }
}
