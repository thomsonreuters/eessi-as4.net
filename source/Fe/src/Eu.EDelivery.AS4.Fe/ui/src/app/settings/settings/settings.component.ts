import { CanComponentDeactivate } from './../../common/candeactivate.guard';
import { Component, OnDestroy, QueryList, ElementRef, ViewChildren } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';

import { Settings } from './../../api/Settings';
import { SettingsStore } from '../settings.store';
import { SettingsService } from '../settings.service';

@Component({
    selector: 'as4-settings',
    templateUrl: './settings.component.html'
})
export class SettingsComponent implements OnDestroy, CanComponentDeactivate {
    public settings: Settings;
    public isDirty: boolean;
    private storeSubscr: Subscription;
    @ViewChildren('dirtycheck') public components: QueryList<CanComponentDeactivate>;
    constructor(appStore: SettingsStore, private settingsService: SettingsService, private elementRef: ElementRef) {
        this.storeSubscr = appStore.changes
            .filter((result) => result != null)
            .subscribe((result) => this.settings = result.Settings);
    }
    public ngOnDestroy() {
        this.storeSubscr.unsubscribe();
    }
    public canDeactivate(): boolean {
        return !!!this.components.find((cmp) => {
            if (cmp.canDeactivate) {
                return !cmp.canDeactivate();
            }
        });
    }
}
