import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, NgZone } from '@angular/core';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { ItemType } from './../../api/ItemType';
import { SettingForm } from './../../api/SettingForm';
import { StepForm } from './../../api/StepForm';
import { DialogService } from './../../common/dialog.service';

@Component({
  selector: 'as4-step-settings',
  templateUrl: './step.component.html',
  styleUrls: ['./step.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StepSettingsComponent {
  @Input() public group: FormArray;
  @Input() public disabled: boolean = true;
  @Input() public steps: ItemType[];

  constructor(
    private formBuilder: FormBuilder,
    private dialogService: DialogService,
    private _changeDetectorRef: ChangeDetectorRef,
    private _ngZone: NgZone
  ) {}

  public itemMoved = () => {
    this._ngZone.run(() => {
      this.group.markAsDirty();
    });
  };

  public addStep() {
    this.group.push(StepForm.getForm(this.formBuilder, null));
    this.group.markAsDirty();
  }

  public removeStep(index: number) {
    if (!this.dialogService.confirm('Are you sure you want to delete the step ?')) {
      return;
    }
    this.group.removeAt(index);
    this.group.markAsDirty();
  }

  public stepChanged(formGroup: FormGroup, selectedStep: string) {
    let stepProps = this.steps.find((st) => st.technicalName === selectedStep);
    const setting = formGroup.get('setting');
    if (setting) {
      formGroup.removeControl('setting');
    }
    if (stepProps && stepProps.properties.length > 0) {
      formGroup.addControl(
        'setting',
        this.formBuilder.array(
          stepProps.properties.map((prop) =>
            SettingForm.getForm(
              this.formBuilder,
              {
                key: prop.technicalName,
                value: prop.defaultValue,
                attributes: prop.attributes
              },
              prop.required
            )
          )
        )
      );
    }
    this._changeDetectorRef.detectChanges();
  }
}
