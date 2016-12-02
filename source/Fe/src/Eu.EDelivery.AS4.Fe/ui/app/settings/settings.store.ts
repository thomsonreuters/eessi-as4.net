import { ISettingsState } from './settings.store';
import { SettingsAgents } from './../api/SettingsAgents';
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
        if (type === SettingsAgents.FIELD_receptionAwarenessAgent) this.setReceptionAwarenessAgent(agent);
        else this.setAgent(agent, originalName, type);
        this.setState(this.state);
    }
    public deleteAgent(type: string, agent: SettingsAgent) {
        if (type === SettingsAgents.FIELD_receptionAwarenessAgent) this.state.Settings.agents.receptionAwarenessAgent = undefined;
        else this.removeAgent(agent, type);
        this.setState(this.state);
    }
    public addAgent(type: string, agent: SettingsAgent) {
        if (type === SettingsAgents.FIELD_receptionAwarenessAgent) this.state.Settings.agents.receptionAwarenessAgent = agent;
        else this.state.Settings.agents[type] = [...this.state.Settings.agents[type], agent];
        this.setState(this.state);
    }
    private setReceptionAwarenessAgent(settingsAgent: SettingsAgent) {
        this.state.Settings.agents[SettingsAgents.FIELD_receptionAwarenessAgent] = settingsAgent;
    }
    private setAgent(settingsAgent: SettingsAgent, originalName: string, type: string) {
        this.state.Settings.agents[type] = <SettingsAgent[]>this.state.Settings.agents[type].map(agt => agt.name === originalName ? Object.assign({}, settingsAgent) : agt);
    }
    private removeAgent(settingsAgent: SettingsAgent, type: string) {
        let state = <SettingsAgent[]>this.state.Settings.agents[type];
        this.state.Settings.agents[type] = state.filter(agt => agt.name !== settingsAgent.name);
        this.setState(this.state);
    }
}
