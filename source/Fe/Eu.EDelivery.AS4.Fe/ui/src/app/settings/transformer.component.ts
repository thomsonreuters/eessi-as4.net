import { Component, Input, Output, forwardRef, OnDestroy, AfterViewInit } from '@angular/core';
import { FormGroup, FormArray, FormBuilder } from '@angular/forms';
import { Subscription } from 'rxjs/Subscription';

import { RuntimeStore } from './runtime.store';
import { Receiver } from './../api/Receiver';
import { SettingForm } from './../api/SettingForm';
import { ItemType } from './../api/ItemType';
import { SettingsService } from './settings.service';
import { Observable } from 'rxjs/Observable';

@Component({
    selector: 'as4-transformer',
    template: `
        <div [formGroup]="group">
            <as4-input [label]="'Type'">
                <select class="form-control" formControlName="type" (change)="transformerChanged($event.target.value)" #type required>
                    <option *ngFor="let type of types" [value]="type.technicalName">{{type.name}}</option>
                </select>
            </as4-input>
            <as4-runtime-settings *ngIf="!!types" [form]="group.get('setting')" labelSize="3" controlSize="6" [types]="types" [itemType]="group.controls['type'].value"></as4-runtime-settings>      
        </div>
    `
})
export class TransformerComponent {
    @Input() public group: FormGroup;
    @Input() public types: ItemType[];
    public currentTransformer: ItemType | undefined;
    
    constructor(private settingsService: SettingsService, private formBuilder: FormBuilder) { 
        
     }

    public transformerChanged(value: string) {
        this.currentTransformer = this.types.find((transformer) => transformer.technicalName === value);
        this.group.removeControl('setting');

        if (!!!this.currentTransformer || !!!this.currentTransformer.properties) {
            return;
        }

        this.group
            .addControl('setting', this.formBuilder.array(this.currentTransformer
                .properties
                .map((prop) => SettingForm.getForm(this.formBuilder, {
                    key: prop.technicalName,
                    value: prop.defaultValue,
                    attributes: prop.attributes
                }, prop.required))));
    }
}
