import { Method } from './../../api/Method';
import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { FormGroup, FormBuilder, FormArray, AbstractControl } from '@angular/forms';

import { ParameterForm } from './../../api/ParameterForm';
import { DialogService } from './../../common/dialog.service';
import { ItemType } from './../../api/ItemType';

@Component({
    selector: 'as4-method',
    template: ` 
        <div [formGroup]="group">
            <as4-input [label]="label" [runtimeTooltip]="runtime">
                <select class="form-control" (change)="typeChanged($event.target.value)" as4-auth formControlName="type">
                    <option value="">Select a value</option>
                    <option *ngFor="let type of types" [value]="type.name">{{type.name}}</option>
                </select>
            </as4-input>
            <as4-input label="Method parameters" *ngIf="!!parametersControl && parametersControl.length > 0" runtimeTooltip="method.parameters" [isLabelBold]="false">
                <table class="table" formArrayName="parameters">
                    <colgroup>
                        <col width="30%">
                        <col width="70%">
                    </colgroup>
                    <tr>
                        <th>Name</th>
                        <th>Value</th>
                    </tr>
                    <tr *ngFor="let setting of parametersControl; let i = index" [formGroupName]="i">
                        
                        <td as4-tooltip="{{(types | gettype:group.get('type')?.value | getitemtypeproperty:parametersControl[i].value.name)?.description}}">{{(types | gettype:group.get('type')?.value | getitemtypeproperty:parametersControl[i].value.name)?.friendlyName}}</td> 
                        <td><input type="text" name="value" class="value-input form-control" formControlName="value"/></td>
                    </tr>
                </table>
            </as4-input>
        </div>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class MethodComponent {
    @Input() public group: FormGroup;
    @Input() public types: ItemType[];
    @Input() public isDisabled: boolean = false;
    @Input() public label: string;
    @Input() public runtime: string;
    public currentType: ItemType | undefined;
    public get parametersControl(): any {
        return !!this.group && (<FormGroup>this.group!.get('parameters'))!.controls;
    }
    constructor(private formBuilder: FormBuilder, private dialogService: DialogService) { }
    public typeChanged(result: string) {
        this.currentType = this.types.find((method) => method.name === result);
        this.group.setControl(Method.FIELD_parameters, this.formBuilder.array(!!!this.currentType || !!!this.currentType.properties ? [] : this.currentType.properties.map(prop => ParameterForm.getForm(this.formBuilder, {
            name: prop.technicalName.toLowerCase(),
            value: ''
        }))));
    }
}
