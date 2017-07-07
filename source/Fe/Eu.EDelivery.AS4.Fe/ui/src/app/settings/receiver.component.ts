import { Component, Input, Output, forwardRef, OnDestroy } from '@angular/core';
import { FormGroup, FormArray, FormBuilder } from '@angular/forms';
import { Subscription } from 'rxjs/Subscription';

import { RuntimeStore } from './runtime.store';
import { Receiver } from './../api/Receiver';
import { SettingForm } from './../api/SettingForm';
import { ItemType } from './../api/ItemType';

@Component({
    selector: 'as4-receiver',
    template: `
        <div [formGroup]="group">    
            <as4-input [label]="'Type'">
                <select class="form-control" formControlName="type" (change)="receiverChanged($event.target.value)" #type>
                    <option *ngFor="let type of types" [value]="type.technicalName">{{type.name}}</option>
                </select>
            </as4-input>
            <as4-runtime-settings [form]="group" [types]="types" [itemType]="type.value"></as4-runtime-settings>      
        </div>
    `
})
export class ReceiverComponent implements OnDestroy {
    @Input() public group: FormGroup;
    public types: ItemType[];
    public currentReceiver: ItemType | undefined;
    private _runtimeStoreSubscription: Subscription;
    constructor(private runtimeStore: RuntimeStore, private formBuilder: FormBuilder) {
        this._runtimeStoreSubscription = this.runtimeStore
            .changes
            .filter((result) => result != null)
            .subscribe((result) => this.types = result.receivers);
    }
    public receiverChanged(value: string) {
        this.currentReceiver = this.types.find((receiver) => receiver.technicalName === value);
        this.group.removeControl('setting');

        if (!!!this.currentReceiver || !!!this.currentReceiver.properties) {
            return;
        }

        console.log(this.currentReceiver);
        this.group
            .addControl('setting', this.formBuilder.array(this.currentReceiver
                .properties
                .map((prop) => SettingForm.getForm(this.formBuilder, {
                    key: prop.friendlyName,
                    value: ''
                }))));
    }
    public ngOnDestroy() {
        this._runtimeStoreSubscription.unsubscribe();
    }
}
