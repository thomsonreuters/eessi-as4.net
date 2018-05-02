import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ItemType } from './../api/ItemType';
import { SettingForm } from './../api/SettingForm';

@Component({
  selector: 'as4-transformer',
  template: `
        <div [formGroup]="group">
            <as4-input [label]="'Type'">
                <select class="form-control" formControlName="type" (change)="transformerChanged($event.target.value)" #type required>
                    <option *ngFor="let type of types" [value]="type.technicalName">{{type.name}}</option>
                </select>
            </as4-input>
            <as4-runtime-settings *ngIf="types" [form]="group.get('setting')" labelSize="3" controlSize="6" [types]="types" [itemType]="group.controls['type'].value"></as4-runtime-settings>      
        </div>
    `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TransformerComponent {
  @Input() public group: FormGroup;
  @Input() public types: ItemType[];

  constructor(private formBuilder: FormBuilder) {}

  public transformerChanged(value: string) {
    const currentTransformer = this.types.find((transformer) => transformer.technicalName === value);
    this.group.removeControl('setting');

    if (!currentTransformer || !currentTransformer.properties) {
      return;
    }

    this.group.addControl(
      'setting',
      this.formBuilder.array(
        currentTransformer.properties.map((prop) =>
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
}
