import { CanComponentDeactivate } from './../common/candeactivate.guard';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Component, Input, Output } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { Observable } from 'rxjs/Observable';

import { DialogService } from './../common/dialog.service';
import { SettingsService } from './settings.service';
import { SettingsDatabase } from './../api/SettingsDatabase';
import { SettingsDatabaseForm } from './../api/SettingsDatabaseForm';
import '../common/rxjs/toBehaviorSubject';

@Component({
    selector: 'as4-database-settings',
    template: `
        <form [formGroup]="form" class="form-horizontal">
            <as4-input [label]="'Provider'">
                <input type="text" class="form-control pull-right" name="provider" (keydown.enter)="save()" formControlName="provider"/>
            </as4-input>
            <as4-input [label]="'Connectionstring'">
                <input type="text" class="form-control pull-right" name="provider" (keydown.enter)="save()" formControlName="connectionString"/>
            </as4-input>
        </form>
    `
})
export class DatabaseSettingsComponent implements CanComponentDeactivate {
    @Input() public get settings(): SettingsDatabase {
        return this._settings;
    }
    public set settings(settingsDatabase: SettingsDatabase) {
        this.form = SettingsDatabaseForm.getForm(this.formBuilder, settingsDatabase);
        this._settings = settingsDatabase;
        this.isDirty = this.form.valueChanges.map(() => this.form.dirty).toBehaviorSubject(this.form.dirty);
    }
    @Output() public isDirty: Observable<boolean>;
    public form: FormGroup;
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
            .subscribe((result) => {
                if (result) {
                    this.form.markAsPristine();
                    this.form.updateValueAndValidity();
                    this.dialogService.message(`Settings will only be applied after restarting the runtime!`, 'Attention');
                }
            });
    }
    public canDeactivate(): boolean {
        return !this.form.dirty;
    }
}
