import { Observer } from 'rxjs/Observer';
import { Observable } from 'rxjs/Observable';

import { SettingsAgent } from './../api/SettingsAgent';
import { SettingsDatabase } from './../api/SettingsDatabase';
import { CustomSettings } from './../api/CustomSettings';
import { Base } from './../api/Base';
import { ISettingsService } from './settings.service';

export class SettingsServiceMock implements ISettingsService {
    public getSettings() { }
    public saveBaseSettings(base: Base) { }
    public saveCustomSettings(custom: CustomSettings): Observable<boolean> {
        return Observable.of(false);
    }
    public saveDatabaseSettings(settings: SettingsDatabase): Observable<boolean> {
        return Observable.of(false);
    }
    public createAgent(settings: SettingsAgent, agent: string): Observable<boolean> {
        return Observable.of(false);
    }
    public updateAgent(settings: SettingsAgent, originalName: string, agent: string): Observable<boolean> {
        return Observable.of(false);
    }
    public deleteAgent(settings: SettingsAgent, agent: string) { }
}
