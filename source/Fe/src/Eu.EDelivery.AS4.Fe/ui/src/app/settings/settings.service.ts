import { SettingsAgent } from './../api/SettingsAgent';
import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';

import { SettingsStore } from './settings.store';
import { Base } from './../api/Base';
import { CustomSettings } from './../api/CustomSettings';
import { SettingsDatabase } from './../api/SettingsDatabase';

export interface ISettingsService {
    getSettings();
    saveBaseSettings(base: Base);
    saveCustomSettings(custom: CustomSettings): Observable<boolean>;
    saveDatabaseSettings(settings: SettingsDatabase): Observable<boolean>;
    createAgent(settings: SettingsAgent, agent: string): Observable<boolean>;
    updateAgent(settings: SettingsAgent, originalName: string, agent: string): Observable<boolean>;
    deleteAgent(settings: SettingsAgent, agent: string);
}

@Injectable()
export class SettingsService implements ISettingsService {
    constructor(private http: AuthHttp, private settingsStore: SettingsStore) {
        this.getSettings();
    }
    public getSettings() {
        return this
            .http
            .get(this.getUrl())
            .subscribe(result => this.settingsStore.update('Settings', result.json()));
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
                subj.complete();
            });
        return subj.asObservable();
    }
    public createAgent(settings: SettingsAgent, agent: string): Observable<boolean> {
        let subject = new Subject<boolean>();
        this.http
            .post(this.getUrl(agent), settings)
            .subscribe(() => {
                this.settingsStore.addAgent(agent, settings);
                subject.next(true);
                subject.complete();
            }, () => {
                subject.next(false);
                subject.complete();
            });
        return subject.asObservable();
    }
    public updateAgent(settings: SettingsAgent, originalName: string, agent: string): Observable<boolean> {
        let subject = new Subject<boolean>();

        // Make a copy of the SettingsAgent and swap the originalName with the new name
        // This is done because the api expects the new name as a route parameter instead of the old name
        this.http
            .put(`${this.getUrl(agent)}/${originalName}`, settings)
            .subscribe(() => {
                this.settingsStore.updateAgent(agent, originalName, settings);
                subject.next(true);
                subject.complete();
            }, () => {
                subject.next(false);
                subject.complete();
            });
        return subject.asObservable();
    }
    public deleteAgent(settings: SettingsAgent, agent: string) {
        this.http
            .delete(`${this.getUrl(agent)}?name=${settings.name}`, settings)
            .subscribe(() => this.settingsStore.deleteAgent(agent, settings));
    }

    private getUrl(path?: string): string {
        if (path === undefined) return '/api/configuration';
        else return `/api/configuration/${path}`;
    }
}
