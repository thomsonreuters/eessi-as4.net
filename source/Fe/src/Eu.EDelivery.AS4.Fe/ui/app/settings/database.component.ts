import { Component, Input } from '@angular/core';

import { SettingsService } from './settings.service';
import { SettingsDatabase } from './../api/SettingsDatabase';

@Component({
    selector: 'as4-database-settings',
    template: `
        <div class="form-group">
            <label>Provider</label>
            <input type="text" class="form-control pull-right" id="provider" (keydown.enter)="save()" [(ngModel)]="settings && settings.provider"/>
        </div>
        <div class="form-group">
            <label>Connectionstring</label>
            <input type="text" class="form-control pull-right" id="provider" (keydown.enter)="save()" [(ngModel)]="settings && settings.connectionString"/>
        </div>
    `
})
export class DatabaseSettingsComponent {
    @Input() settings: SettingsDatabase;
    constructor(private settingsService: SettingsService) {

    }
    public save() {
        this.settingsService.saveDatabaseSettings(this.settings);
    }
}
