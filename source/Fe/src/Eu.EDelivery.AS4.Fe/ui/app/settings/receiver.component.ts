import { RuntimeStore } from './runtime.store';
import { Component, Input, Output, forwardRef, OnDestroy } from '@angular/core';
import { FormGroup, FormArray, FormBuilder } from '@angular/forms';
import { Subscription } from 'rxjs/Subscription';

import { Receiver } from './../api/Receiver';
import { Setting } from './../api/Setting';
import { ItemType } from './../api/ItemType';

@Component({
    selector: 'as4-receiver',
    template: `
        <div [formGroup]="group">    
            <as4-input [label]="'Type'">
                <select class="form-control" formControlName="type" (change)="receiverChanged($event.target.value)">
                    <option *ngFor="let type of types" [value]="type.technicalName">{{type.name}}</option>
                </select>
            </as4-input>
            <div *ngIf="group.controls.setting.controls.length > 0">
                <h4>Settings</h4>
                <table class="table table-condensed" formArrayName="setting">
                    <tbody>
                        <tr *ngFor="let set of group.controls.setting.controls; let i = index" [formGroupName]="i">
                            <td>{{set.value.key}}&nbsp;<as4-info [tooltip]="currentReceiver && currentReceiver.properties[i] && currentReceiver.properties[i].description"></as4-info></td>
                            <td><input type="text" class="form-control" formControlName="value"/><td>
                        </tr>
                    </tbody>
                </table>
            </div>             
        </div>    
    `
})
export class ReceiverComponent implements OnDestroy {
    @Input() group: FormGroup;
    public types: ItemType[];
    public currentReceiver: ItemType;
    private _runtimeStoreSubscription: Subscription;
    constructor(private runtimeStore: RuntimeStore, private formBuilder: FormBuilder) {
        this._runtimeStoreSubscription = this.runtimeStore
            .changes
            .filter(result => result != null)
            .subscribe(result => this.types = result.receivers);
    }
    public receiverChanged(value: string) {
        this.currentReceiver = this.types.find(receiver => receiver.technicalName === value);
        this.group.removeControl('setting');

        if (!!!this.currentReceiver || !!!this.currentReceiver.properties) return;

        console.log(this.currentReceiver);
        this.group
            .addControl('setting', this.formBuilder.array(this.currentReceiver
                .properties
                .map(prop => Setting.getForm(this.formBuilder, {
                    key: prop.friendlyName,
                    value: ''
                }))));
    }
    public ngOnDestroy() {
        this._runtimeStoreSubscription.unsubscribe();
    }
}
