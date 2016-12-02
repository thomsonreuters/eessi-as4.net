import { Component } from '@angular/core';
import { Routes } from '@angular/router';
import { MustBeAuthorizedGuard } from '../common/common.guards';

import { AgentSettingsComponent } from './agent.component';
import { SettingsComponent } from './settings.component';
import { ReceptionAwarenessAgentComponent } from './receptionawarenessagent.component';
import { WrapperComponent } from './../common/wrapper.component';

export const ROUTES: Routes = [
    {
        path: 'settings', component: WrapperComponent, children: [
            { path: '', redirectTo: 'common' },
            { path: 'common', component: SettingsComponent, data: { title: 'Base settings' } },
            {
                path: 'agents', data: { title: 'Agents' }, children: [
                    { path: '', redirectTo: 'submit', pathMatch: 'full' },
                    { path: 'submit', component: AgentSettingsComponent, data: { title: 'Submit', type: 'submitAgents' } },
                    { path: 'receive', component: AgentSettingsComponent, data: { title: 'Receive', type: 'receiveAgents' } },
                    { path: 'send', component: AgentSettingsComponent, data: { title: 'Send', type: 'sendAgents' } },
                    { path: 'deliver', component: AgentSettingsComponent, data: { title: 'Deliver', type: 'deliverAgents' } },
                    { path: 'notify', component: AgentSettingsComponent, data: { title: 'Notify', type: 'notifyAgents' } },
                    { path: 'receptionawareness', component: ReceptionAwarenessAgentComponent, data: { title: 'Reception', type: 'receptionAwarenessAgent' } }
                ]
            }
        ],
        data: { title: 'Settings' },
        canActivate: [MustBeAuthorizedGuard]
    }
];
