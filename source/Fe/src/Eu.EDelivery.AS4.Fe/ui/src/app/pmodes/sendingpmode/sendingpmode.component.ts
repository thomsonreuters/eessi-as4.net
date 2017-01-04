import { SendingProcessingMode } from './../../api/SendingProcessingMode';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs/Observable';

import { BasePmodeComponent } from './../basepmode/basepmode.component';
import { SendingPmode } from './../../api/SendingPmode';
import { ReceivingPmode } from './../../api/ReceivingPmode';

@Component({
    selector: 'as4-sending-pmode',
    templateUrl: './sendingpmode.component.html'
})
export class SendingPmodeComponent extends BasePmodeComponent<SendingPmode> {
    patchForm(formBuilder: FormBuilder, form: FormGroup, pmode: SendingPmode) {
        SendingPmode.patchForm(this.formBuilder, this.form, this.currentPmode);
    }
    newPmode(newName: string): SendingPmode {
        let newPmode = new SendingPmode();
        newPmode.name = newName;
        newPmode.pmode = new SendingProcessingMode();
        newPmode.pmode.id = newName;
        newPmode.type = 1;
        return newPmode;
    }
    init() {
        this.form = SendingPmode.getForm(this.formBuilder, null);
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
            .map(result => result.SendingNames)
            .distinctUntilChanged()
            .subscribe(result => this.pmodes = result);
        this._currentPmodeSubscription = this.pmodeStore
            .changes
            .filter(result => !!result)
            .map(result => result.Sending)
            .distinctUntilChanged()
            .subscribe(result => {
                this.currentPmode = result;
                SendingPmode.patchForm(this.formBuilder, this.form, result);
            });
    }
    getPmode(pmode: string) {
        this.pmodeService.setSending(pmode);
    }
    setPmode(pmode: SendingPmode | undefined) {
        this.pmodeStore.setSending(pmode);
    }
    createPmode(value: any): Observable<boolean> {
        return this.pmodeService.createSending(value);
    }
    updatePmode(value: any, originalName: string): Observable<boolean> {
        return this.pmodeService.updateSending(value, originalName);
    }
    deletePmode(value: any) {
        this.pmodeService.deleteSending(value);
    }
    getByName(name: string): Observable<SendingPmode> {
        return this.pmodeService.getSendingByName(name);
    }
}
