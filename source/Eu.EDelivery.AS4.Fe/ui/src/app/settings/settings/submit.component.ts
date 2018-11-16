import { Component, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

import { SettingsSubmit } from '../../api/SettingsSubmit';
import { SettingsSubmitForm } from '../../api/SettingsSubmitForm';
import { CanComponentDeactivate } from '../../common/candeactivate.guard';
import { DialogService } from '../../common/dialog.service';
import { SettingsService } from '../settings.service';

@Component({
  selector: 'as4-submit-settings',
  template: `
        <form [formGroup]="form" class="form-horizontal">
            <as4-input [label]="'Payload Retrieval Path'">
                <input 
                    data-cy="payloadRetrieval"
                    type="text"
                    class="form-control pull right"
                    name="payloadRetrievalPath"
                    (keydown.enter)="save()"
                    formControlName="payloadRetrievalPath" />
            </as4-input>
        </form>`
})
export class SubmitSettingsComponent implements CanComponentDeactivate {
  @Output()
  public isDirty: Observable<boolean>;
  public form: FormGroup;
  private _settings: SettingsSubmit;
  constructor(
    private settingsService: SettingsService,
    private formBuilder: FormBuilder,
    private dialogService: DialogService
  ) {}
  @Input()
  public get settings(): SettingsSubmit {
    return this._settings;
  }
  public set settings(settingsSubmit: SettingsSubmit) {
    this.form = SettingsSubmitForm.getForm(this.formBuilder, settingsSubmit);
    this._settings = settingsSubmit;
    this.isDirty = this.form.valueChanges
      .map(() => this.form.dirty)
      .toBehaviorSubject(this.form.dirty);
  }
  public save() {
    if (!this.form.valid) {
      this.dialogService.incorrectForm();
    }
    this.settingsService
      .saveSubmitSettings(this.form.value)
      .subscribe((result) => {
        if (result) {
          this.form.markAsPristine();
          this.form.updateValueAndValidity();
          this.dialogService.message(
            'Settings will only be applied after restarting the runtime',
            'Attention'
          );
        }
      });
  }
  public canDeactivate(): boolean {
    return !this.form.dirty;
  }
}
