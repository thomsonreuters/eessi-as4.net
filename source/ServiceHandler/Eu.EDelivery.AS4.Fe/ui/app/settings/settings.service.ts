import { SettingsAgent } from './../api/SettingsAgent';
import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';

import { SettingsStore } from './settings.store';
import { Base } from './../api/Base';
import { CustomSettings } from './../api/CustomSettings';
import { SettingsDatabase } from './../api/SettingsDatabase';

@Injectable()
export class SettingsService {
    constructor(private http: AuthHttp, private settingsStore: SettingsStore) {
        this.getSettings();
    }
    public getSettings() {
        return this
            .http
            .get(this.getUrl())
            .subscribe(result => {
                this.settingsStore.update('Settings', result.json());
            });
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
        var subject = new Subject<boolean>();
        if (!Array.isArray(settings.receiver.text)) {
            var fixup = new Array<string>();
            fixup.push(settings.receiver.text);
            settings.receiver.text = fixup;
        }

        this.http
            .post(this.getUrl('submitagents'), settings)
            .subscribe(() => {
                subject.next(true);
                subject.complete();
            }, () => {
                subject.next(false);
                subject.complete();
            });
        return subject.asObservable();
    }
    private getUrl(path?: string): string {
        if (path === undefined) return '/api/configuration';
        else return `/api/configuration/${path}`;
    }
}