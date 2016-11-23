import { Component, Input, Output, forwardRef } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { Receiver } from './../api/Receiver';

@Component({
    selector: 'as4-receiver',
    template: `
        <div [formGroup]="group">
            <h3>Receiver</h3>
            <p>Type: <input type="text" class="form-control" formControlName="type"/></p>
            <p>Text: <input type="text" class="form-control" formControlName="text" /></p>
            <h3>Settings</h3>
            <table class="table table-striped" formArrayName="setting">
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
    constructor() {
    }
}