import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';

import { SettingsStore, StoreHelper } from './settings.store';
import { Base } from './../api/Base';
import { CustomSettings } from './../api/CustomSettings';
import { SettingsDatabase } from './../api/SettingsDatabase';

@Injectable()
export class SettingsService {
    constructor(private http: AuthHttp, private settingsStore: SettingsStore, private storeHelper: StoreHelper) {

    }
    public getSettings() {
        return this
            .http
            .get(this.getUrl())
            .subscribe(result => this.storeHelper.update('Settings', result.json()));
    }
    public saveBaseSettings(base: Base) {
        this.http
            .post(this.getUrl('basesettings'), base)
            .subscribe(() => {
                alert('Saved');
            });
    }
    public saveCustomSettings(custom: CustomSettings) {
        this.http
            .post(this.getUrl('customsettings'), custom)
            .subscribe(() => {
                alert('saved');
            });
    }
    public saveDatabaseSettings(settings: SettingsDatabase) {
        this.http
            .post(this.getUrl('databasesettings'), settings)
            .subscribe(() => {
                alert('Saved');
            });
    }
    private getUrl(path?: string): string {
        if (path === undefined) return '/api/configuration';
        else return `/api/configuration/${path}`;
    }
}