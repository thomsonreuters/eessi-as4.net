import { Observer } from 'rxjs/Observer';
import { Observable } from 'rxjs/Observable';

import { SettingsAgent } from './../api/SettingsAgent';
import { SettingsDatabase } from './../api/SettingsDatabase';
import { CustomSettings } from './../api/CustomSettings';
import { Base } from './../api/Base';
import { ISettingsService } from './settings.service';

export class SettingsServiceMock implements ISettingsService {
    getSettings() {

    }
    saveBaseSettings(base: Base) {

    }
    saveCustomSettings(custom: CustomSettings): Observable<boolean> {
        return null;
    }
    saveDatabaseSettings(settings: SettingsDatabase): Observable<boolean> {
        return null;
    }
    createAgent(settings: SettingsAgent, agent: string): Observable<boolean> {
        return null;
    }
    updateAgent(settings: SettingsAgent, originalName: string, agent: string): Observable<boolean> {
        return null;
    }
    deleteAgent(settings: SettingsAgent, agent: string) {

    }
}
