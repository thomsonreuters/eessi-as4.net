import { Component, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';

import { Base } from './../api/Base';
import { SettingsService } from './settings.service';

@Component({
    selector: 'as4-base-settings',
    template: `
        <form [formGroup]="form" class="form-horizontal">
            <as4-input [label]="'Id format'">
                <input type="text" class="form-control pull-right" id="idFormat" formControlName="idFormat"/>
            </as4-input>
            <as4-input [label]="'Certificate store name'">
                <input type="text" class="form-control pull-right" id="certificateStoreName" (keydown.enter)="save()" formControlName="certificateStoreName"/>
            </as4-input>
        </form>
    `
})
export class BaseSettingsComponent {
    private _settings: Base;
    private form: FormGroup;
    @Input() public get settings(): Base {
        return this._settings;
    }
    public set settings(baseSetting: Base) {
        this.form = Base.getForm(this.formBuilder, baseSetting);
        this._settings = baseSetting;
    }
    @Output() public get isDirty(): boolean {
        return this.form.dirty;
    }
    constructor(private settingsService: SettingsService, private formBuilder: FormBuilder) {

    }
    public save() {
        this.settingsService
            .saveBaseSettings(this.form.value)
            .subscribe(result => this.form.markAsPristine());
    }
}
