import { DynamicDiscovery } from './../../api/DynamicDiscovery';
import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { FormGroup, FormBuilder, FormArray, AbstractControl, FormControlName } from '@angular/forms';

import { DynamicDiscoverySettingForm } from './../../api/DynamicDiscoverySettingForm';
import { DialogService } from './../../common/dialog.service';
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
    constructor(private formBuilder: FormBuilder, private dialogService: DialogService) { }
    public typeChanged(result: string) {
        console.log(result);
        this.currentType = this.types.find((profile) => profile.name === result);
        console.log(this.currentType);
        this.group.setControl(
            DynamicDiscovery.FIELD_settings, 
            this.formBuilder.array(
                !!!this.currentType || !!!this.currentType.properties 
                    ? [] 
                    : this.currentType.properties
                        .map(prop => DynamicDiscoverySettingForm
                            .getForm(this.formBuilder, { key: prop.technicalName.toLowerCase(), value: '' }))));
    }
}
