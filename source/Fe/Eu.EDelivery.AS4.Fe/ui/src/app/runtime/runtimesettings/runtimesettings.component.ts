import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { ItemType } from './../../api/ItemType';
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
          <p>{{form.value | json}}</p>
    `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RuntimeSettingsComponent implements OnChanges {
  @Input() public form: FormArray;
  @Input() public types: ItemType[];
  @Input() public controlSize = 8;
  @Input() public labelSize = 4;
  @Input() public itemType: string;
  @Input() public pshowTitle: boolean = true;

  public selectedType: ItemType | undefined;

  constructor(private _formBuilder: FormBuilder) {}

  public onSettingChange() {
    if (!!this.selectedType) {
      const list = this.form.controls.map((form: FormGroup) => form.controls['key'].value);
      for (const prop of this.selectedType.properties) {
        if (!!!list.find((search) => search === prop.technicalName)) {
          // Add it
          this.form.push(
            SettingForm.getForm(this._formBuilder, {
              key: prop.technicalName,
              value: prop.defaultValue,
              attributes: prop.attributes
            })
          );
        }
      }
    }
  }

  public ngOnChanges(changes: SimpleChanges) {
    const { itemType } = changes;
    if (itemType) {
      if (this.types) {
        this.selectedType = this.types!!.find((type) => type.technicalName === itemType.currentValue);
      }
      this.onSettingChange();
    }
  }
}
