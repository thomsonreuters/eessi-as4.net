import { Component, Input, Output } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';

import { DialogService } from './../common/dialog.service';
import { SettingsService } from './settings.service';
import { SettingsDatabase } from './../api/SettingsDatabase';

@Component({
    selector: 'as4-database-settings',
    template: `
        <form [formGroup]="form" class="form-horizontal">
            <as4-input [label]="'Provider'">
                <input type="text" class="form-control pull-right" id="provider" (keydown.enter)="save()" formControlName="provider"/>
            </as4-input>
            <as4-input [label]="'Connectionstring'">
                <input type="text" class="form-control pull-right" id="provider" (keydown.enter)="save()" formControlName="connectionString"/>
            </as4-input>
        </form>
    `
})
export class DatabaseSettingsComponent {
    @Input() public get settings(): SettingsDatabase {
        return this._settings;
    }
    public set settings(settingsDatabase: SettingsDatabase) {
        this.form = SettingsDatabase.getForm(this.formBuilder, settingsDatabase);
        this._settings = settingsDatabase;
    }
    @Output() get isDirty(): boolean {
        return this.form.dirty;
    }
    private form: FormGroup;
    private _settings: SettingsDatabase;
    constructor(private settingsService: SettingsService, private formBuilder: FormBuilder, private dialogService: DialogService) {

    }
    public save() {
        if (!this.form.valid) {
            this.dialogService.incorrectForm();
            return;
        }
        this.settingsService
            .saveDatabaseSettings(this.form.value)
            .subscribe(result => {
                if (result) this.form.markAsPristine();
            });
    }
}
