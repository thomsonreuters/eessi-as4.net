import { Transformer, TransformerConfigEntry } from './../api/Transformer';
import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';

import { SettingsStore } from './settings.store';
import { Base } from './../api/Base';
import { CustomSettings } from './../api/CustomSettings';
import { SettingsDatabase } from './../api/SettingsDatabase';
import { PortalSettings } from './../api/PortalSettings';
import { SettingsAgent } from './../api/SettingsAgent';
import { Steps } from './../api/Steps';
import { ItemType } from '../api/ItemType';
import { StepPipeline } from '../api/StepPipeline';

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
    constructor(private http: AuthHttp, private settingsStore: SettingsStore) { }
    public getSettings() {
        return this
            .http
            .get(this.getUrl())
            .subscribe((result) => this.settingsStore.update('Settings', result.json()));
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
    public getDefaultAgentSteps(agentType: number): Observable<StepPipeline> {
        return this
            .http
            .get(this.getUrl('defaultagentsteps') + '/' + agentType)
            .map((result) => result.json());
    }
    public getDefaultAgentTransformer(agentType: number): Observable<TransformerConfigEntry> {
        return this
            .http
            .get(this.getUrl('defaultagenttransformer') + '/' + agentType)
            .map((result) => result.json());
    }
    public createAgent(settings: SettingsAgent, agent: string): Observable<boolean> {
        return this.http
            .post(this.getUrl(agent), settings)
            .do(() => this.settingsStore.updateAgent(agent, null, settings))
            .map(() => true)
            .catch(() => Observable.of(false));
    }
    public updateAgent(settings: SettingsAgent, originalName: string, agent: string): Observable<boolean> {
        // Make a copy of the SettingsAgent and swap the originalName with the new name
        // This is done because the api expects the new name as a route parameter instead of the old name
        return this.http
            .put(`${this.getUrl(agent)}/${originalName}`, settings)
            .do(() => this.settingsStore.updateAgent(agent, originalName, settings))
            .map(() => true)
            .catch(() => Observable.of(false));
    }
    public deleteAgent(settings: SettingsAgent, agent: string) {
        this.http
            .delete(`${this.getUrl(agent)}?name=${settings.name}`, { body: settings })
            .subscribe(() => this.settingsStore.deleteAgent(agent, settings));
    }
    public getPortalSettings(): Observable<PortalSettings> {
        return this
            .http
            .get(this.getUrl('portal'))
            .map((result) => result.json());
    }
    public savePortalSettings(settings: PortalSettings): Observable<boolean> {
        return this
            .http
            .post(this.getUrl('portal'), settings)
            .map((result) => true);
    }
    private getUrl(path?: string): string {
        if (path === undefined) {
            return '/api/configuration';
        } else { return `/api/configuration/${path}`; }
    }
}
