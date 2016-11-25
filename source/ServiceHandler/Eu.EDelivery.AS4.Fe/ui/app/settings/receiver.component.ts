import { RuntimeStore } from './runtime.store';
import { Component, Input, Output, forwardRef } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { Receiver } from './../api/Receiver';
import { RuntimeService, ItemType } from './runtime.service';

@Component({
    selector: 'as4-receiver',
    template: `
        <div [formGroup]="group">
            <as4-input [label]="'Type'">
                <select class="form-control" formControlName="type">
                    <option *ngFor="let type of types">{{type.name}}</option>
                </select>
            </as4-input>
            <as4-input [label]="'Text'">
                <input type="text" class="form-control" formControlName="text" />
            </as4-input>
            <h4>Settings</h4>
            <table class="table table-condensed" formArrayName="setting">
                <tbody>
                    <tr *ngFor="let set of group.controls.setting.controls; let i = index" [formGroupName]="i">
                        <td>{{set.value.key}}</td>
                        <td><input type="text" class="form-control" formControlName="value"/><td>
                    </tr>
                </tbody>
            </table>             
        </div>    
    `
})
export class ReceiverComponent {
    @Input() group: FormGroup;
    public types: ItemType[];
    constructor(private runtimeService: RuntimeService, private runtimeStore: RuntimeStore) {
        this.runtimeStore
            .changes
            .filter(result => result != null)
            .subscribe(result => {
                this.types = result.receivers;
            });
        this.runtimeService.getReceivers();
    }
}