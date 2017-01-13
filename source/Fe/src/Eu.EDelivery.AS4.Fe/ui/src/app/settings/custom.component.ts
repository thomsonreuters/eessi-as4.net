import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Component, Input, Output } from '@angular/core';
import { Observable } from 'rxjs/Observable';

import { DialogService } from './../common/dialog.service';
import { CustomSettings } from './../api/CustomSettings';
import { SettingsService } from './settings.service';
import { Setting } from './../api/Setting';

@Component({
    selector: 'as4-custom-settings',
    template: `
        <form [formGroup]="form">
            <as4-input>
                <p><button type="button" class="btn btn-flat" (click)="addSetting()"><i class="fa fa-plus"></i></button></p>
                <table class="table table-condensed" formArrayName="setting">
                    <tr>
                        <th>Key</th>
                        <th>Value</th>
                    </tr>
                    <tr *ngFor="let step of form.get('setting'); let i = index" [formGroupName]="i">
                        <td class="action"><button type="button" class="btn btn-flat" (click)="removeSetting(i)"><i class="fa fa-trash-o"></i></button></td>
                        <td><input type="text" class="form-control" formControlName="key"/></td>
                        <td><input type="text" class="form-control" formControlName="value"/></td>
                    </tr>
                </table>
            </as4-input>
        </form>
    `
})
export class CommonSettingsComponent {
    public form: FormGroup;
    @Input() public get settings(): CustomSettings {
        return this._settings;
    }
    @Output() public get isDirty(): Observable<boolean> {
        return Observable.of<boolean>(this.form.dirty);
    }
    public set settings(settings: CustomSettings) {
        this.form = CustomSettings.getForm(this.formBuilder, settings);
        this._settings = settings;
    }
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
            .subscribe(result => {
                if (result) this.form.markAsPristine();
            });
    }
    public addSetting() {
        this.form.markAsDirty();
        let settings = <FormArray>this.form.controls['setting'];
        settings.push(Setting.getForm(this.formBuilder, new Setting()));
    }
    public removeSetting(index: number) {
        if (!this.dialogService.confirm('Please confirm that you want to delete the setting')) {
            return;
        }
        (<FormArray>this.form.controls['setting']).removeAt(index);
        this.form.markAsDirty();
    }
}
