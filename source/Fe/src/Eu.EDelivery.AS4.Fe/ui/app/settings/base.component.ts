import { Component, Input } from '@angular/core';

import { Base } from './../api/Base';
import { SettingsService } from './settings.service';

@Component({
    selector: 'as4-base-settings',
    template: `
        <div class="form-group">
            <label>Id format</label>
            <input type="text" class="form-control pull-right" id="idFormat" [(ngModel)]="settings && settings.idFormat"/>
        </div>
        <div class="form-group">
            <label>Certificate store name</label>
            <input type="text" class="form-control pull-right" id="certificateStoreName" (keydown.enter)="save()" [(ngModel)]="settings && settings.certificateStoreName"/>
        </div>
    `
})
export class BaseSettingsComponent {
    @Input() settings: Base;
    constructor(private settingsService: SettingsService) {

    }
    public save() {
        let setting = new Base();
        setting.idFormat = this.settings.idFormat;
        setting.certificateStoreName = this.settings.certificateStoreName;
        this.settingsService.saveBaseSettings(setting);
    }
}
