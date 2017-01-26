import { CanDeactivateGuard } from './../common/candeactivate.guard';
import { Component } from '@angular/core';
import { Routes } from '@angular/router';
import { MustBeAuthorizedGuard } from '../common/mustbeauthorized.guard';

import { AgentSettingsComponent } from './agent/agent.component';
import { SettingsComponent } from './settings/settings.component';
import { ReceptionAwarenessAgentComponent } from './receptionawarenessagent/receptionawarenessagent.component';
import { WrapperComponent } from './../common/wrapper.component';

export const ROUTES: Routes = [
    {
        path: '',
        component: WrapperComponent, children: [
            { path: 'inbound', component: AgentSettingsComponent, data: { title: 'Inbound', type: 'receiveAgents' }, canActivate: [MustBeAuthorizedGuard], canDeactivate: [CanDeactivateGuard] },
            { path: 'outbound', component: AgentSettingsComponent, data: { title: 'Outbound', type: 'submitAgents' }, canActivate: [MustBeAuthorizedGuard], canDeactivate: [CanDeactivateGuard] },
            {
                path: 'settings', children: [
                    { path: '', redirectTo: 'common', pathMatch: 'full', canDeactivate: [CanDeactivateGuard] },
                    { path: 'common', component: SettingsComponent, data: { title: 'Base settings' }, canDeactivate: [CanDeactivateGuard] },
                    {
                        path: 'agents', data: { title: 'Agents' }, children: [
                            { path: '', redirectTo: 'submit', pathMatch: 'full', canDeactivate: [CanDeactivateGuard] },
                            { path: 'submit', component: AgentSettingsComponent, data: { title: 'Submit', type: 'submitAgents' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'send', component: AgentSettingsComponent, data: { title: 'Send', type: 'sendAgents' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'receive', component: AgentSettingsComponent, data: { title: 'Receive', type: 'receiveAgents' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'deliver', component: AgentSettingsComponent, data: { title: 'Deliver', type: 'deliverAgents' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'notify', component: AgentSettingsComponent, data: { title: 'Notify', type: 'notifyAgents' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'receptionawareness', component: ReceptionAwarenessAgentComponent, data: { title: 'Reception', type: 'receptionAwarenessAgent' }, canDeactivate: [CanDeactivateGuard] }
                        ]
                    }
                ],
                data: { title: 'Settings' },
                canDeactivate: [CanDeactivateGuard]
            }
        ],
        canActivate: [MustBeAuthorizedGuard]
    }
];
