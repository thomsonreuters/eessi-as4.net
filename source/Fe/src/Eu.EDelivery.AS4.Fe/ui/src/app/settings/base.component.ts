import { Observable } from 'rxjs';
import { Component, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';

import { DialogService } from './../common/dialog.service';
import { RuntimeStore } from './runtime.store';
import { Base } from './../api/Base';
import { SettingsService } from './settings.service';
import { ItemType } from './../api/ItemType';

@Component({
    selector: 'as4-base-settings',
    template: `
        <form [formGroup]="form" class="form-horizontal">
            <as4-input [label]="'Id format'">
                <input type="text" class="form-control" name="idFormat" formControlName="idFormat"/>
            </as4-input>
            <as4-input [label]="'Store name'" formGroupName="certificateStore">
                <input type="text" class="form-control" name="certificateStoreName" formControlName="storeName"/>
            </as4-input>
            <div formGroupName="certificateStore">
                <as4-input [label]="'Repository type'" formGroupName="repository">
                     <select class="form-control" formControlName="type">
                        <option *ngFor="let repository of repositories" [value]="repository.technicalName">{{repository.name}}</option>
                    </select>
                </as4-input>
            </div>
        </form>
    `
})
export class BaseSettingsComponent {
    public repositories: ItemType[];
    public form: FormGroup;
    private _settings: Base;
    @Input() public get settings(): Base {
        return this._settings;
    }
    @Output() public get isDirty(): Observable<boolean> {
        return Observable.of<boolean>(this.form.dirty);
    }
    public set settings(baseSetting: Base) {
        this.form = Base.getForm(this.formBuilder, baseSetting);
        this._settings = baseSetting;
    }
    constructor(private settingsService: SettingsService, private formBuilder: FormBuilder, private runtimeStore: RuntimeStore, private dialogService: DialogService) {
        runtimeStore
            .changes
            .filter(result => !!(result && result.certificateRepositories))
            .subscribe(result => {
                this.repositories = result.certificateRepositories;
            });
    }
    public save() {
        if (!this.form.valid) {
            this.dialogService.incorrectForm();
            return;
        }
        this.settingsService
            .saveBaseSettings(this.form.value)
            .subscribe(result => this.form.markAsPristine());
    }
}
