import { Component, Input, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';
import { FormGroup, FormArray, FormBuilder, FormControl } from '@angular/forms';

import { Setting } from './../../api/Setting';
import { ItemType } from './../../api/ItemType';
import { Property } from './../../api/Property';
import { SettingForm } from './../../api/SettingForm';

@Component({
    selector: 'as4-runtime-settings',
    template: `
          <div *ngIf="!!form && !!form.value && !!form.controls && form.controls.length > 0">
            <h4 *ngIf="showTitle === true">Settings</h4>
            <div *ngFor="let setting of form.controls; let i = index">
                <as4-runtime-setting [control]="setting" [controlSize]="controlSize" [labelSize]="labelSize" [runtimeType]="selectedType | getvalue:setting.value.key"></as4-runtime-setting>
            </div>
          </div>

    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RuntimeSettingsComponent {
    @Input() public form: FormArray;
    @Input() public types: ItemType[];
    @Input() public controlSize = '8';
    @Input() public labelSize = '4';
    @Input() public set itemType(newType: string) {
        if (this._type !== newType) {
            this.selectedType = this.types.find((type) => type.technicalName === newType);
            this._type = newType;
        }
    }
    @Input() public pshowTitle: boolean = true;
    public selectedType: ItemType | undefined;
    private _type: string;
    constructor(private _formBuilder: FormBuilder) { }
    public onSettingChange(setting: Setting, property: Property) {
        // Check if the formGroup already has the setting
        const settingArray = <FormArray>this.form.get(`setting`);
        const exists = settingArray.controls.find((ctrl) => (<Setting>ctrl.value).key === setting.key);

        // formGroup doesn't have the setting ... create it
        if (!!!exists) {
            settingArray.push(SettingForm.getForm(this._formBuilder, setting, property.required));
            this.form.markAsDirty();
            return;
        }

        // It has the setting, update the value
        exists.setValue(setting);
        this.form.markAsDirty();
    }
}
