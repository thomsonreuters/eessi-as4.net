import { CanComponentDeactivate } from './../common/candeactivate.guard';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { FormBuilder, FormGroup, FormArray, AbstractControl } from '@angular/forms';
import { Component, Input, Output } from '@angular/core';
import { Observable } from 'rxjs/Observable';

import { DialogService } from './../common/dialog.service';
import { CustomSettingsForm } from './../api/CustomSettingsForm';
import { CustomSettings } from './../api/CustomSettings';
import { SettingsService } from './settings.service';
import '../common/rxjs/toBehaviorSubject';
import { SettingForm } from '../api/SettingForm';
import { Setting } from '../api/Setting';

@Component({
    selector: 'as4-custom-settings',
    template: `
        <form [formGroup]="form">
            <as4-input>
                <p><button type="button" as4-auth class="btn btn-flat" (click)="addSetting()"><i class="fa fa-plus"></i></button></p>
                <table class="table table-condensed" formArrayName="setting">
                    <tr>
                        <th>Key</th>
                        <th>Value</th>
                    </tr>
                    <tr *ngFor="let step of settingControl; let i = index" [formGroupName]="i">
                        <td class="action"><button as4-auth type="button" class="btn btn-flat" (click)="removeSetting(i)"><i class="fa fa-trash-o"></i></button></td>
                        <td><input type="text" class="form-control" formControlName="key"/></td>
                        <td><input type="text" class="form-control" formControlName="value"/></td>
                    </tr>
                </table>
            </as4-input>
        </form>
    `
})
export class CommonSettingsComponent implements CanComponentDeactivate {
    public form: FormGroup;
    public get settingControl(): { [key: string]: AbstractControl } {
        return !!this.form && (<FormGroup>this.form.get('setting')).controls;
    }
    @Input() public get settings(): CustomSettings {
        return this._settings;
    }
    public set settings(settings: CustomSettings) {
        this.form = CustomSettingsForm.getForm(this.formBuilder, settings);
        this._settings = settings;
        this.isSaveEnabled = this.form.valueChanges.map(() => this.form.dirty && this.form.valid);
    }
    @Output() public isSaveEnabled: Observable<boolean>;
    private _settings: CustomSettings;
    constructor(private settingsService: SettingsService, private formBuilder: FormBuilder, private dialogService: DialogService) {
    }
    public save() {
        if (!this.form.valid) {
            this.dialogService.incorrectForm();
            return;
        }
        this.settingsService
            .saveCustomSettings(this.form.value)
            .subscribe((result) => {
                if (result) {
                    this.form.markAsPristine();
                    this.form.updateValueAndValidity();
                    this.dialogService.message(`Settings will only be applied after restarting the runtime!`, 'Attention');
                }
            });
    }
    public addSetting() {
        this.form.markAsDirty();
        let settings = <FormArray>this.form.controls['setting'];
        settings.push(SettingForm.getForm(this.formBuilder, new Setting()));
    }
    public removeSetting(index: number) {
        if (!this.dialogService.confirm('Please confirm that you want to delete the setting')) {
            return;
        }
        (<FormArray>this.form.controls['setting']).removeAt(index);
        this.form.markAsDirty();
        this.form.updateValueAndValidity();
    }
    public canDeactivate(): boolean {
        return !this.form.dirty;
    }
}
