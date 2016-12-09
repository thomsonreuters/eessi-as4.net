import { Component, Input, SimpleChanges } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { ItemType } from './../api/ItemType';

@Component({
    selector: 'as4-runtime-settings',
    template: `
        <form [formGroup]="form">
            <div *ngIf="form.controls.setting.controls.length > 0">
                <h4 *ngIf="showTitle === true">Settings</h4>
                <table class="table table-condensed" formArrayName="setting">
                    <tbody>
                        <tr *ngFor="let set of form.controls.setting.controls; let i = index" [formGroupName]="i">
                            <td>{{set.value.key}}&nbsp;<as4-info [tooltip]="selectedType && selectedType.properties[i] && selectedType.properties[i].description"></as4-info></td>
                            <td [ngSwitch]="selectedType && selectedType.properties[i] && selectedType.properties[i].type">
                                <input *ngSwitchCase="'int'" type="number" class="form-control" formControlName="value"/>
                                <input *ngSwitchDefault type="text" class="form-control" formControlName="value"/>
                            <td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </form>
    `
})
export class RuntimeSettingsComponent {
    @Input() form: FormGroup;
    @Input() types: ItemType[];
    @Input() itemType: string;
    @Input() showTitle: boolean = true;
    public selectedType: ItemType;
    ngOnChanges(changes: SimpleChanges) {
        let itemType = changes['itemType'] && changes['itemType'].currentValue;
        if (!!!this.types) return;
        this.selectedType = this.types.find(type => type.technicalName === itemType);
    }
}
