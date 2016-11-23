import { SettingsAgent } from './../api/SettingsAgent';
import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';

import { SettingsStore, StoreHelper } from './settings.store';
import { Base } from './../api/Base';
import { CustomSettings } from './../api/CustomSettings';
import { SettingsDatabase } from './../api/SettingsDatabase';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class SettingsService {
    constructor(private http: AuthHttp, private settingsStore: SettingsStore, private storeHelper: StoreHelper) {
        this.getSettings();
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
    public updateOrCreateSubmitAgent(settings: SettingsAgent): Observable<boolean> {
        return Observable.create(obs => {
            if (!Array.isArray(settings.receiver.text)) {
                var fixup = new Array<string>();
                fixup.push(settings.receiver.text);
                settings.receiver.text = fixup;
            }

            this.http
                .post(this.getUrl('submitagents'), settings)
                .subscribe(() => {
                    alert('saved');
                });     

            obs.next(true);           
        });
    }
    private getUrl(path?: string): string {
        if (path === undefined) return '/api/configuration';
        else return `/api/configuration/${path}`;
    }
}