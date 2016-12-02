import { ActivatedRoute } from '@angular/router';
import { SettingsAgents } from './../api/SettingsAgents';
import { SettingsAgent } from './../api/SettingsAgent';
import { Settings } from './../api/Settings';

import { Observable } from 'rxjs';
import {
    inject,
    TestBed
} from '@angular/core/testing';
import { Component } from '@angular/core';
import {
    BaseRequestOptions,
    ConnectionBackend,
    Http
} from '@angular/http';
import { MockBackend } from '@angular/http/testing';

import { SettingsStore } from './settings.store';

describe('settingsstore', () => {
    let submitAgent: SettingsAgent;
    let receptionAwarenessAgent: SettingsAgent;
    let settings: Settings;
    const agentName1: string = 'agentName1';
    const agentName2: string = 'agentName2';
    const receptionAwarenessAgent1: string = 'receptionawarenessagent';
    beforeEach(() => {
        settings = new Settings();
        settings.agents = new SettingsAgents();
        settings.agents.submitAgents = new Array<SettingsAgent>();

        submitAgent = new SettingsAgent();
        submitAgent.name = agentName1;
        settings.agents.submitAgents.push(submitAgent);
        settings.agents.notifyAgents = [];

        receptionAwarenessAgent = new SettingsAgent();
        receptionAwarenessAgent.name = receptionAwarenessAgent1;
        settings.agents.receptionAwarenessAgent = receptionAwarenessAgent;
    });
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            SettingsStore
        ]
    }));

    describe('addagent', () => {
        it('should add the agent', inject([SettingsStore], (settingsStore: SettingsStore) => {
            settingsStore.setState({ Settings: settings });

            let agent = new SettingsAgent();
            agent.name = 'test';

            // Act
            settingsStore.addAgent(SettingsAgents.FIELD_notifyAgents, agent);

            // Assert
            expect(settingsStore.getState().Settings.agents.notifyAgents.find(agt => agt.name === 'test')).toBeDefined();
        }));
        it('should add the reception awareness agent', inject([SettingsStore], (settingsStore: SettingsStore) => {
            settingsStore.setState({ Settings: settings });

            let agent = new SettingsAgent();
            agent.name = 'test';

            // Act
            settingsStore.addAgent(SettingsAgents.FIELD_receptionAwarenessAgent, agent);

            // Assert
            expect(settingsStore.getState().Settings.agents.receptionAwarenessAgent.name === 'test').toBeTruthy();
        }));
    });
    describe('updateagent', () => {
        it('should update the requested agent', inject([SettingsStore], (settingsStore: SettingsStore) => {
            settingsStore.setState({ Settings: settings });

            submitAgent = Object.assign({}, submitAgent);
            submitAgent.name = 'HALLO';

            // Act
            settingsStore.updateAgent(SettingsAgents.FIELD_submitAgents, submitAgent.name, submitAgent);

            // Assert
            expect(settingsStore.getState().Settings.agents.submitAgents.find(agt => agt.name === 'HALLO'));
        }));
        it('should update the receptionawarenessagent', inject([SettingsStore], (settingsStore: SettingsStore) => {
            settingsStore.setState({ Settings: settings });

            receptionAwarenessAgent = Object.assign({}, receptionAwarenessAgent);
            receptionAwarenessAgent.name = 'CHANGED';

            // Act
            settingsStore.updateAgent(SettingsAgents.FIELD_receptionAwarenessAgent, receptionAwarenessAgent1, receptionAwarenessAgent);

            // Assert
            expect(settingsStore.getState().Settings.agents.receptionAwarenessAgent.name === 'CHANGED');
        }));
    });
    describe('deleteagent', () => {
        it('should remove the agent', inject([SettingsStore], (settingsStore: SettingsStore) => {
            settingsStore.setState({ Settings: settings });

            // Act
            settingsStore.deleteAgent(SettingsAgents.FIELD_submitAgents, submitAgent);

            // Assert
            expect(settingsStore.getState().Settings.agents.submitAgents.find(agt => agt.name === submitAgent.name)).toBeUndefined();
        }));
        it('should remove the receptionawarenessagent', inject([SettingsStore], (settingsStore: SettingsStore) => {
            settingsStore.setState({ Settings: settings });

            // Act
            settingsStore.deleteAgent(SettingsAgents.FIELD_receptionAwarenessAgent, receptionAwarenessAgent);

            // Assert
            expect(settingsStore.getState().Settings.agents.receptionAwarenessAgent).toBeUndefined();
        }));
    });
});
