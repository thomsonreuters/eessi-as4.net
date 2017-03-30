import { Method } from './../../api/Method';
import { Component, Input } from '@angular/core';
import { FormGroup, FormBuilder, FormArray, AbstractControl } from '@angular/forms';

import { ParameterForm } from './../../api/ParameterForm';
import { DialogService } from './../../common/dialog.service';
import { ItemType } from './../../api/ItemType';

@Component({
    selector: 'as4-method',
    template: ` 
        <div [formGroup]="group">
            <as4-input [label]="label" runtimeTooltip="method.type">
                <select class="form-control" (change)="typeChanged($event.target.value)" formControlName="type">
                    <option>Select a value</option>
                    <option *ngFor="let type of types" [value]="type.name">{{type.name}}</option>
                </select>
            </as4-input>
            <as4-input label="Method parameters" *ngIf="parametersControl.length > 0" runtimeTooltip="method.parameters">
                <table class="table table-condensed" formArrayName="parameters">
                    <tr>
                        <th>Name</th>
                        <th>Value</th>
                    </tr>
                    <tr *ngFor="let setting of parametersControl; let i = index" [formGroupName]="i">
                        <td>{{parametersControl[i].value.name}}</td>
                        <td><input type="text" name="value" class="value-input form-control" formControlName="value"/></td>
                    </tr>
                </table>
            </as4-input>
        </div>
    `
})
export class MethodComponent {
    @Input() public group: FormGroup;
    @Input() public types: ItemType[];
    @Input() public isDisabled: boolean = false;
    @Input() public label: string;
    public get parametersControl(): any {
        return !!!this.group && this.group.get('parameters');
    }
    constructor(private formBuilder: FormBuilder, private dialogService: DialogService) { }
    public typeChanged(result: string) {
        let type = this.types.find((method) => method.name === result);
        this.group.setControl(Method.FIELD_parameters, this.formBuilder.array(!!!type || !!!type.properties ? [] : type.properties.map(prop => ParameterForm.getForm(this.formBuilder, {
            name: prop.friendlyName,
            value: ''
        }))));
    }
}
