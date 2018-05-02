import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ItemType } from './../api/ItemType';
import { SettingForm } from './../api/SettingForm';

@Component({
  selector: 'as4-receiver',
  template: `
        <div [formGroup]="group">
            <as4-input [label]="'Type'">
                <select class="form-control" formControlName="type" data-cy="receivers" (change)="receiverChanged($event.target.value)" #type required>
                    <option *ngFor="let receiver of receivers" [value]="receiver.technicalName">{{receiver.name}}</option>
                </select>
            </as4-input>
            <as4-runtime-settings [form]="group.get('setting')" labelSize="3" controlSize="6" [types]="receivers" [itemType]="group.controls['type'].value"></as4-runtime-settings>
        </div>
    `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReceiverComponent {
  @Input() public group: FormGroup;
  @Input() public receivers: ItemType[];

  public currentReceiver: ItemType | undefined;

  constructor(private formBuilder: FormBuilder) { }

  public receiverChanged(value: string) {
    this.currentReceiver = this.receivers.find((receiver) => receiver.technicalName === value);
    this.group.removeControl('setting');

    if (!this.currentReceiver || !this.currentReceiver.properties) {
      return;
    }

    this.group.addControl(
      'setting',
      this.formBuilder.array(
        this.currentReceiver.properties.map((prop) =>
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
