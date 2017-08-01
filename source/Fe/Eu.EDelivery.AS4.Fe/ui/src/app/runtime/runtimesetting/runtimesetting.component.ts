import { FormGroup, FormControl, Validators, FormArray } from '@angular/forms';
import { Component, Input, EventEmitter, Output, ChangeDetectionStrategy } from '@angular/core';

import { Setting } from './../../api/Setting';
import { Property } from './../../api/Property';

@Component({
    selector: 'as4-runtime-setting',
    templateUrl: 'runtimesetting.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RuntimeSettingComponent {
    @Input() public control: FormControl;
    @Input() public runtimeType: Property;
    @Input() public value: any;
    @Input() public labelSize: string;
    @Input() public controlSize: string;
    @Output() public onChange = new EventEmitter<Setting>();
    public timeSpanMask = [/[0-9]/, /[0-9]/, ':', /[0-5]/, /[0-9]/, ':', /[0-5]/, /[0-9]/];
    public hasParameters(): boolean {
        let array = (<FormArray>this.control.get('attributes')!).controls;
        if (!!!array) {
            return false;
        }
        return array.length > 0;
    }
}
