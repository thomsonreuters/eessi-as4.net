import { Component } from '@angular/core';

import { Settings } from './../api/Settings';
import { SettingsStore } from './settings.store';
import { SettingsService } from './settings.service';

@Component({
    selector: 'as4-settings',
    templateUrl: './settings.component.html'
})
export class SettingsComponent {
    public settings: Settings;
    constructor(appStore: SettingsStore, private settingsService: SettingsService) {
        settingsService.getSettings();
        appStore.changes.subscribe(result => this.settings = result.Settings);
    }
}