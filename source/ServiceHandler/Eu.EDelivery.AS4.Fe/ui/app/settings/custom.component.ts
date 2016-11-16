import { Component, Input } from '@angular/core';

import { CustomSettings } from './../api/CustomSettings';
import { SettingsService } from './settings.service';

@Component({
    selector: 'as4-custom-settings',
    template: `
        <div class="form-group" *ngFor="let setting of settings?.setting">
            <label>{{setting.key}}</label>
            <input type="text" class="form-control pull-right" [(ngModel)]="setting.value" (keydown.enter)="save()"/>
        </div>
    `
})
export class CommonSettingsComponent {
    @Input() public settings: CustomSettings;
    constructor(private settingsService: SettingsService) {

    }
    public save() {
        this.settingsService.saveCustomSettings(this.settings);
    }
}
