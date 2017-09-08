import { PortalSettingsComponent } from './portalsettings/portalsettings.component';
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
            { path: 'submit', component: AgentSettingsComponent, data: { title: 'Submit Agents', type: 'submitAgents', icon: 'fa-cloud-upload', weight: -10 }, canActivate: [MustBeAuthorizedGuard], canDeactivate: [CanDeactivateGuard] },
            { path: 'receive', component: AgentSettingsComponent, data: { title: 'Receive Agents', type: 'receiveAgents', icon: 'fa-cloud-download', weight: -9 }, canActivate: [MustBeAuthorizedGuard], canDeactivate: [CanDeactivateGuard] },
            {
                path: 'settings', children: [
                    { path: '', redirectTo: 'portal', pathMatch: 'full', canDeactivate: [CanDeactivateGuard] },
                    { path: 'portal', component: PortalSettingsComponent, data: { title: 'Portal settings' }, canDeactivate: [CanDeactivateGuard] },
                    { path: 'runtime', component: SettingsComponent, data: { title: 'Runtime settings' }, canDeactivate: [CanDeactivateGuard] },
                    {
                        path: 'agents', data: { title: 'Internal Agents' }, children: [
                            { path: '', redirectTo: 'submit', pathMatch: 'full', canDeactivate: [CanDeactivateGuard] },
                            { path: 'outboundprocessing', component: AgentSettingsComponent, data: { title: 'Outbound processing', header: 'Outbound processing agent', type: 'outboundProcessingAgents', betype: 8 }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'send', component: AgentSettingsComponent, data: { title: 'Send', header: 'Send agent', type: 'sendAgents', betype: 2 }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'deliver', component: AgentSettingsComponent, data: { title: 'Deliver', header: 'Deliver agent', type: 'deliverAgents', betype: 3 }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'notifyagent', component: AgentSettingsComponent, data: { title: 'Notify agent', header: 'Notify agent', type: 'notifyAgents', betype: 4 }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'receptionawareness', component: ReceptionAwarenessAgentComponent, data: { title: 'Reception awareness', header: 'Reception awareness agent', type: 'receptionAwarenessAgent', betype: 5 }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'pullsend', component: AgentSettingsComponent, data: { title: 'Pull send', header: 'Pull send agent', type: 'pullSendAgents', betype: 7 }, canDeactivate: [CanDeactivateGuard] }
                        ]
                    }
                ],
                data: { title: 'Settings', icon: 'fa-toggle-on' },
                canDeactivate: [CanDeactivateGuard]
            }
        ],
        canActivate: [MustBeAuthorizedGuard]
    }
];
