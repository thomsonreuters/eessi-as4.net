import { Component, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

import { SettingsPullSend } from '../../api/SettingsPullSend';
import { SettingsPullSendForm } from '../../api/SettingsPullSendForm';
import { CanComponentDeactivate } from '../../common/candeactivate.guard';
import { DialogService } from '../../common/dialog.service';
import { SettingsService } from '../settings.service';

@Component({
  selector: 'as4-pullsend-settings',
  template: `
        <form [formGroup]="form" class="form-horizontal">
            <as4-input [label]="'Authorization Map Path'">
                <input 
                    data-cy="authorizationMap"
                    type="text" 
                    class="form-control pull right"
                     name="authorizationMapPath" 
                     (keydown.enter)="save()" 
                     formControlName="authorizationMapPath" />
            </as4-input>
        </form>`
})
export class PullSendSettingsComponent implements CanComponentDeactivate {
  @Output()
  public isDirty: Observable<boolean>;
  public form: FormGroup;
  private _settings: SettingsPullSend;
  constructor(
    private settingsService: SettingsService,
    private formBuilder: FormBuilder,
    private dialogService: DialogService
  ) {}
  @Input()
  public get settings(): SettingsPullSend {
    return this._settings;
  }
  public set settings(settingsPullSend: SettingsPullSend) {
    this.form = SettingsPullSendForm.getForm(
      this.formBuilder,
      settingsPullSend
    );
    this._settings = settingsPullSend;
    this.isDirty = this.form.valueChanges
      .map(() => this.form.dirty)
      .toBehaviorSubject(this.form.dirty);
  }
  public save() {
    if (!this.form.valid) {
      this.dialogService.incorrectForm();
      return;
    }
    this.settingsService
      .savePullSendSettings(this.form.value)
      .subscribe((result) => {
        if (result) {
          this.form.markAsPristine();
          this.form.updateValueAndValidity();
          this.dialogService.message(
            `Settings will only be applied after restarting the runtime!`,
            'Attention'
          );
        }
      });
  }
  public canDeactivate(): boolean {
    return !this.form.dirty;
  }
}
