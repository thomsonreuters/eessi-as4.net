import { ModalService } from './../../common/modal/modal.service';
import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import {
    inject,
    TestBed
} from '@angular/core/testing';
import { FormBuilder } from '@angular/forms';
import { Component } from '@angular/core';
import {
    BaseRequestOptions,
    ConnectionBackend,
    Http
} from '@angular/http';
import { MockBackend } from '@angular/http/testing';

import { SettingsAgents } from './../../api/SettingsAgents';
import { ReceptionAwarenessAgentComponent } from './receptionawarenessagent.component';
import { SettingsStore } from '../settings.store';
import { SettingsAgent } from './../../api/SettingsAgent';
import { Settings } from './../../api/Settings';
import { SettingsServiceMock } from '../settings.service.mock';
import { SettingsService } from '../settings.service';
import { DialogService } from './../../common/dialog.service';
import { RuntimeServiceMock } from '../runtime.service.mock';
import { RuntimeService } from '../runtime.service';
import { ItemType } from './../../api/ItemType';
import { RuntimeStore } from '../runtime.store';

describe('receptionawarenessAgent', () => {
    const currentAgentName: string = 'currentAgent';
    let currentAgent: SettingsAgent;
    let otherAgent: SettingsAgent;
    let agents: Array<SettingsAgent>;
    let settings: Settings;
    beforeEach(() => {
        currentAgent = new SettingsAgent();
        currentAgent.name = currentAgentName;
        otherAgent = new SettingsAgent();
        otherAgent.name = 'otherAgent';
        agents = new Array<SettingsAgent>();
        agents.push(currentAgent);
        agents.push(otherAgent);

        settings = new Settings();
        settings.agents = new SettingsAgents();
        settings.agents.receptionAwarenessAgent = currentAgent;
    });
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            ReceptionAwarenessAgentComponent,
            SettingsStore,
            RuntimeStore,
            FormBuilder,
            DialogService,
            ModalService,
            { provide: RuntimeService, useClass: RuntimeServiceMock },
            { provide: SettingsService, useClass: SettingsServiceMock }
        ]
    }));
    describe('when created', () => {
        it('should select the first available agent', inject([ReceptionAwarenessAgentComponent, SettingsStore, SettingsService], (agent: ReceptionAwarenessAgentComponent, settingsStore: SettingsStore, settingsService: SettingsService) => {
            // Act
            settingsStore.setState({ Settings: settings });

            // Assert 
            expect(agent.currentAgent).toBe(currentAgent);
            expect(agent.form.value.name).toBe(currentAgent.name);
        }));
        it('currentAgent should not be undefined', inject([ReceptionAwarenessAgentComponent], (agent: ReceptionAwarenessAgentComponent) => {
            expect(agent.currentAgent).toBeDefined();
        }));
        it('should load transformers', inject([ReceptionAwarenessAgentComponent, RuntimeStore], (agent: ReceptionAwarenessAgentComponent, runtimeStore: RuntimeStore) => {
            let transformers = new Array<ItemType>();
            // Act
            runtimeStore.setState({
                receivers: new Array<ItemType>(),
                steps: new Array<ItemType>(),
                transformers: transformers,
                certificateRepositories: new Array<ItemType>(),
                deliverSenders: new Array<ItemType>(),
                runtimeMetaData: new Array<any>()
            });

            // Assert
            expect(agent.transformers).toBe(transformers);
        }));
    });
    describe('save', () => {
        it('should call runtimeservice.updateAgent when already saved and form should be marked pristine', inject([ReceptionAwarenessAgentComponent, SettingsService, SettingsStore], (agent: ReceptionAwarenessAgentComponent, settingsService: SettingsService, settingsStore: SettingsStore) => {
            let observable = Observable.of(true);
            let settingsServiceSpy = spyOn(settingsService, 'updateAgent').and.returnValue(observable);

            settingsStore.setState({ Settings: settings });

            let form = {
                valid: true,
                markAsPristine: () => { }
            };
            let formSpy = spyOn(form, 'markAsPristine');
            agent.form = <any>form;

            // Act
            agent.save();

            // Assert
            expect(settingsServiceSpy).toHaveBeenCalled();
            expect(formSpy).toHaveBeenCalled();
        }));
        it('should show a message when the form is invalid and no call to settingsService', inject([ReceptionAwarenessAgentComponent, DialogService, SettingsService], (agent: ReceptionAwarenessAgentComponent, dialogService: DialogService, settingsService: SettingsService) => {
            let form = {
                valid: false
            };
            agent.form = <any>form;
            let dialogServiceSpy = spyOn(dialogService, 'incorrectForm');
            let settingsServiceSpy = spyOn(settingsService, 'updateAgent').and.throwError('I SHOULD NOT HAVE BEEN CALLED');

            // Act
            agent.save();

            // Assert
            expect(dialogServiceSpy).toHaveBeenCalled();
        }));
        it('should update the currentAgent value after save', inject([ReceptionAwarenessAgentComponent, SettingsStore, SettingsService], (agent: ReceptionAwarenessAgentComponent, settingsStore: SettingsStore, settingsService: SettingsService) => {
            settingsStore.setState({ Settings: settings });
            let observable = Observable.of(true);
            let settingsServiceSpy = spyOn(settingsService, 'updateAgent').and.returnValue(observable);
            let settingsServiceGetSpy = spyOn(settingsService, 'getSettings').and.returnValue(observable).and.callFake(() => {
                settingsStore.setState({ Settings: settings });
            });
            let form = {
                valid: true,
                markAsPristine: () => { },
                value: {
                    name: 'FDSQFDSQFDSQ'
                }
            };
            agent.form = <any>form;

            // Act
            agent.save();
            agent.reset();

            // Assert
            expect(agent.currentAgent.name).toBe('FDSQFDSQFDSQ');
        }));
        it('should call createAgent when in new mode', inject([ReceptionAwarenessAgentComponent, SettingsService], (agent: ReceptionAwarenessAgentComponent, settingsService: SettingsService) => {
            let observable = Observable.of(true);
            let settingsSpy = spyOn(settingsService, 'createAgent').and.returnValue(observable);
            let settingsUpdateSpy = spyOn(settingsService, 'updateAgent').and.throwError('I SHOULD NOT HAVE BEEN CALLED');
            let form = {
                valid: true,
                markAsPristine: () => { }
            };
            agent.form = <any>form;
            let formSpy = spyOn(form, 'markAsPristine');

            // Act
            agent.save();

            // Assert
            expect(settingsSpy).toHaveBeenCalledWith(agent.currentAgent, SettingsAgents.FIELD_receptionAwarenessAgent);
            expect(formSpy).toHaveBeenCalled();
        }));
    });
    describe('reset', () => {
        it('should revert the form value back to its original value', inject([ReceptionAwarenessAgentComponent, SettingsStore], (agent: ReceptionAwarenessAgentComponent, settingsStore: SettingsStore) => {
            agent.currentAgent = currentAgent;
            settingsStore.setState({
                Settings: settings
            });

            // Act
            agent.form.value.name = 'test';
            expect(agent.currentAgent.name).toBe(currentAgentName);
            agent.reset();

            // Assert
            expect(agent.currentAgent.name).toBe(currentAgentName);
            expect(agent.form.value.name).toBe(currentAgentName);
        }));
    });
    describe('delete', () => {
        it('should remove the agent', inject([ReceptionAwarenessAgentComponent, SettingsStore, SettingsService, DialogService], (agent: ReceptionAwarenessAgentComponent, settingsStore: SettingsStore, settingsService: SettingsService, dialogService: DialogService) => {
            settingsStore.setState({ Settings: settings });
            agent.currentAgent = currentAgent;

            let dialogSpy = spyOn(dialogService, 'deleteConfirm').and.returnValue(true);
            let serviceSpy = spyOn(settingsService, 'deleteAgent').and.callFake(() => settingsStore.deleteAgent(SettingsAgents.FIELD_receptionAwarenessAgent, currentAgent));

            // Act
            agent.delete();

            // Assert
            expect(serviceSpy).toHaveBeenCalled();
            expect(agent.currentAgent).toBeDefined();
            expect(agent.currentAgent.name).toBeUndefined();
        }));
        it('should ask for confirmation', inject([ReceptionAwarenessAgentComponent, DialogService], (agent: ReceptionAwarenessAgentComponent, dialogService: DialogService) => {
            let dialogSpy = spyOn(dialogService, 'deleteConfirm');

            // Act
            agent.delete();

            // Assert
            expect(dialogSpy).toHaveBeenCalled();
        }));
    });
});
