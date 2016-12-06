import { Subscription } from 'rxjs/Subscription';
import { FormGroup, FormBuilder } from '@angular/forms';
import { Component, OnInit } from '@angular/core';

import { ReceivingPmode } from './../api/ReceivingPmode';
import { PmodesModule } from './pmodes.module';
import { PmodeStore } from './pmode.store';
import { PmodeService } from './pmode.service';

@Component({
    selector: 'as4-receiving-pmode',
    template: `
        <form [formGroup]="form" class="form-horizontal">
            <div formGroupName="pmode">
                <as4-box>
                    <div content>
                    <p>{{pmodes | json}}</p>
                        <select class="form-control" (change)="pmodeChanged($event.target.value)">
                            <option>Select an option</option>
                            <option *ngFor="let pmode of pmodes">{{pmode}}</option>
                        </select>
                    </div>
                </as4-box>
                <as4-box>
                    <div content class="col-md-6 col-xs-12">
                        <as4-input label="Id">
                            <input type="text" class="form-control" formControlName="id"/>
                        </as4-input>
                        <as4-input label="Message exchange pattern">
                            <select formControlName="mep" class="form-control">
                                <option value="0">One way</option>
                                <option value="1">Two way</option>
                            </select>
                        </as4-input>
                        <as4-input label="Mep binding">
                            <select formControlName="mepBinding" class="form-control">
                                <option value="0">Pull</option>
                                <option value="1">Push</option>
                            </select>
                        </as4-input>
                        <as4-input label="Reliability - duplicate elimination" formGroupName="reliability">
                            <div formGroupName="duplicateElimination" class="checkbox">
                                <input type="checkbox" formControlName="isEnabled"/>
                            </div>
                        </as4-input>
                    </div>
                </as4-box>
                <as4-box title="Receipt handling">
                    <div content formGroupName="receiptHandling" class="col-md-6 col-xs-12">
                        <as4-input label="Use NNR format">
                            <input type="checkbox" formControlName="useNNRFormat"/>
                        </as4-input>
                        <as4-input label="Reply patter">
                            <select formControlName="replyPattern" class="form-control">
                                <option value="0">Response</option>
                                <option value="1">Callback</option>
                            </select>
                        </as4-input>
                        <as4-input label="Callback url">
                            <input type="text" class="form-control" formControlName="callbackUrl"/>
                        </as4-input>
                        <as4-input label="Sending PMode">
                            <input type="text" class="form-control" formControlName="sendingPMode"/>
                        </as4-input>
                    </div>
                </as4-box>
                <as4-box title="Error handling">
                    <div content formGroupName="errorHandling" class="col-md-6 col-xs-12">
                        <as4-input label="Use soap fault">
                            <input type="text" formControlName="useSoapFault" class="form-control">
                        </as4-input>
                        <as4-input label="Reply pattern">
                            <select formControlName="replyPattern" class="form-control">
                                <option value="0">Response</option>
                                <option value="1">Callback</option>
                            </select>
                        </as4-input>
                        <as4-input label="Callback url">
                            <input type="text" formControlName="callbackUrl" class="form-control">
                        </as4-input>
                        <as4-input label="Response http code">
                            <input type="text" formControlName="responseHttpCode" class="form-control">
                        </as4-input>
                        <as4-input label="Sending pmode">
                            <input type="text" formControlName="sendingPMode" class="form-control">
                        </as4-input>
                    </div>
                </as4-box>
            </div>
        </form>
    `
})
export class ReceivingPmodeComponent implements OnInit {
    public form: FormGroup;
    public pmodes: string[];
    private currentPmode: ReceivingPmode;
    private _storeSubscription: Subscription;
    private _currentPmodeSubscription: Subscription;
    constructor(private formBuilder: FormBuilder, private pmodeService: PmodeService, private pmodeStore: PmodeStore) {
        this.form = ReceivingPmode.getForm(this.formBuilder, null);
        this._storeSubscription = this.pmodeStore
            .changes
            .filter(result => !!(result && result.ReceivingNames))
            .map(result => result.ReceivingNames)
            .subscribe(result => {
                this.pmodes = result;
            });
        this._currentPmodeSubscription = this.pmodeStore
            .changes
            .filter(result => !!(result && result.Receiving))
            .map(result => result.Receiving)
            .subscribe(result => this.currentPmode = result);
        this.pmodeService.getAllReceiving();
    }
    public pmodeChanged(name: string) {
        this.pmodeService.getReceiving(name);

        // this.currentPmode = this.pmodes.find(pmode => pmode.name === value);
        // this.form = ReceivingPmode.getForm(this.formBuilder, this.currentPmode);
    }
    ngOnInit() {
    }
    ngOnDestroy() {
        this._storeSubscription.unsubscribe();
        this._currentPmodeSubscription.unsubscribe();
    }
}
