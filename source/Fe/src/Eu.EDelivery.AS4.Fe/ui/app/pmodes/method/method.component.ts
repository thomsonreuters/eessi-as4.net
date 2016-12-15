import { Method } from './../../api/Method';
import { Component, Input } from '@angular/core';
import { FormGroup, FormBuilder, FormArray } from '@angular/forms';

import { Parameter } from './../../api/Parameter';
import { DialogService } from './../../common/dialog.service';
import { ItemType } from './../../api/ItemType';

@Component({
    selector: 'as4-method',
    template: ` 
        <div [formGroup]="group">
            <as4-input [label]="label">
                <select class="form-control" (change)="typeChanged($event.target.value)" formControlName="type">
                    <option>Select a value</option>
                    <option *ngFor="let type of types" [value]="type.name">{{type.name}}</option>
                </select>
            </as4-input>
            <as4-input *ngIf="group.controls.parameters.controls.length > 0">
                <table class="table table-condensed" formArrayName="parameters">
                    <tr>
                        <th>Name</th>
                        <th>Value</th>
                    </tr>
                    <tr *ngFor="let setting of group.controls.parameters.controls; let i = index" [formGroupName]="i">
                        <td>{{group.controls.parameters.controls[i].value.name}}</td>
                        <td><input type="text" name="value" class="value-input form-control" formControlName="value"/></td>
                    </tr>
                </table>
            </as4-input>
        </div>
    `
})
export class MethodComponent {
    @Input() group: FormGroup;
    @Input() types: Array<ItemType>;
    @Input() isDisabled: boolean = false;
    @Input() label: string;
    constructor(private formBuilder: FormBuilder, private dialogService: DialogService) {
    }
    typeChanged(result: string) {
        let type = this.types.find(method => method.name === result);
        this.group.removeControl(Method.FIELD_parameters);
        this.group.addControl(Method.FIELD_parameters, this.formBuilder.array(type.properties.map(prop => Parameter.getForm(this.formBuilder, {
            name: prop.friendlyName,
            value: ''
        }))));
    }
    public forLoaded() {
        alert('forLoaded');
    }
}
