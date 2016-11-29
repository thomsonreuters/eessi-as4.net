import { Component, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';

import { Settings } from './../api/Settings';
import { SettingsStore } from './settings.store';
import { SettingsService } from './settings.service';

@Component({
    selector: 'as4-settings',
    templateUrl: './settings.component.html'
})
export class SettingsComponent implements OnDestroy {
    public settings: Settings;

    private storeSubscr: Subscription;
    constructor(appStore: SettingsStore, private settingsService: SettingsService) {
        this.storeSubscr = appStore.changes
            .filter(result => result != null)
            .subscribe(result => this.settings = result.Settings);
    }

    public ngOnDestroy() {
        this.storeSubscr.unsubscribe();
    }
}
