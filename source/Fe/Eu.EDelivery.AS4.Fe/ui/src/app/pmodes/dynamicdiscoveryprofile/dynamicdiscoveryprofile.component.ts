import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';

import { DynamicDiscovery } from './../../api/DynamicDiscovery';
import { DynamicDiscoverySettingForm } from './../../api/DynamicDiscoverySettingForm';
import { ItemType } from './../../api/ItemType';

@Component({
    selector: 'as4-dynamicdiscovery',
    templateUrl: './dynamicdiscoveryprofile.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DynamicDiscoveryComponent {
    @Input() public group: FormGroup;
    @Input() public types: ItemType[];
    @Input() public isDisabled: boolean = false;
    @Input() public label: string;
    @Input() public runtime: string;
    public currentType: ItemType | undefined;
    public get settingsControl(): any {
        return !!this.group && (<FormGroup>this.group!.get('settings'))!.controls;
    }
    constructor(private formBuilder: FormBuilder) { }
    public typeChanged(result: string) {
        this.currentType = this.types.find((profile) => profile.technicalName === result);
        this.group.setControl(DynamicDiscovery.FIELD_settings, this.formBuilder.array(
                !!!this.currentType || !!!this.currentType.properties
                    ? []
                    : this
                        .currentType
                        .properties
                        .map((prop) => DynamicDiscoverySettingForm
                        .getForm(this.formBuilder, {
                            key: prop.technicalName.toLowerCase(),
                            value: prop.defaultValue
                        })))
                );
    }
}
