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
    public saveBaseSettings(base: Base): Observable<boolean> {
        let subj = new Subject<boolean>();
        this.http
            .post(this.getUrl('basesettings'), base)
            .subscribe(() => {
                subj.next(true);
                subj.complete();
            }, () => {
                subj.next(false);
                subj.complete();
            });
        return subj.asObservable();
    }
    public saveCustomSettings(custom: CustomSettings): Observable<boolean> {
        let subj = new Subject<boolean>();
        this.http
            .post(this.getUrl('customsettings'), custom)
            .subscribe(() => {
                subj.next(true);
                subj.complete();
            }, () => {
                subj.next(false);
                subj.complete();
            });
        return subj.asObservable();
    }
    public saveDatabaseSettings(settings: SettingsDatabase): Observable<boolean> {
        let subj = new Subject<boolean>();
        this.http
            .post(this.getUrl('databasesettings'), settings)
            .subscribe(() => {
                subj.next(true);
                subj.complete();
            }, () => {
                subj.next(false);
                subj.complete()
            });
        return subj.asObservable();
    }
    public updateOrCreateSubmitAgent(settings: SettingsAgent): Observable<boolean> {
        let subject = new Subject<boolean>();
        if (!Array.isArray(settings.receiver.text)) {
            let fixup = new Array<string>();
            if (!!settings.receiver.text) {
                fixup.push(settings.receiver.text);
            }
            <any>settings.receiver.text = fixup;
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
