import { Injectable } from '@angular/core';

import { Store } from './../common/store';
import { Settings } from './../api/Settings';
import { SettingsAgent } from './../api/SettingsAgent';

export interface ISettingsState {
    Settings: Settings;
}

@Injectable()
export class SettingsStore extends Store<ISettingsState> {
    constructor() {
        super({
            Settings: new Settings()
        });
    }
    public updateAgent(type: string, originalName: string, agent: SettingsAgent) {
        let state = this.getState();
        let agents = <SettingsAgent[]>state.Settings.agents[type];
        state.Settings.agents[type] = agents.map(agt => agt.name === originalName ? Object.assign({}, agent) : agt);
        this.setState(state);
    }
}
