import { Injectable } from '@angular/core';
import { Store } from './../common/store';
import { Settings } from './../api/Settings';

export interface ISettingsState {
    Settings: Settings;
}

@Injectable()
export class SettingsStore extends Store<ISettingsState> {
    constructor() {
        super({
            Settings: new Settings()
        })
    }
}