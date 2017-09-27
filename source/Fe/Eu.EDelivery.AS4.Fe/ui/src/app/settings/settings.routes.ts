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
        path: 'submit',
        component: WrapperComponent, children: [
            { path: '', component: AgentSettingsComponent, data: { title: 'Submit Agents', type: 'submitAgents', icon: 'fa-cloud-upload', weight: -10, betype: 0 }, canActivate: [MustBeAuthorizedGuard], canDeactivate: [CanDeactivateGuard] },
        ]
    },
    {
        path: 'receive',
        component: WrapperComponent, children: [
            { path: '', component: AgentSettingsComponent, data: { title: 'Receive Agents', type: 'receiveAgents', icon: 'fa-cloud-download', weight: -9, betype: 1 }, canActivate: [MustBeAuthorizedGuard], canDeactivate: [CanDeactivateGuard] },
        ]
    },
    {
        path: 'settings',
        component: WrapperComponent, children: [
            { path: '', redirectTo: 'portal', pathMatch: 'full', canDeactivate: [CanDeactivateGuard] },
            { path: 'portal', component: PortalSettingsComponent, data: { title: 'Portal settings' }, canDeactivate: [CanDeactivateGuard] },
            { path: 'runtime', component: SettingsComponent, data: { title: 'Runtime settings' }, canDeactivate: [CanDeactivateGuard] },
            {
                path: 'agents', data: { title: 'Internal Agents' }, children: [
                    { path: '', redirectTo: 'submit', pathMatch: 'full', canDeactivate: [CanDeactivateGuard] },
                    { path: 'outboundprocessing', component: AgentSettingsComponent, data: { title: 'Outbound processing', header: 'Outbound processing agent', type: 'outboundProcessingAgents', betype: 8, showwarning: true }, canDeactivate: [CanDeactivateGuard] },
                    { path: 'send', component: AgentSettingsComponent, data: { title: 'Send', header: 'Send agent', type: 'sendAgents', betype: 2, showwarning: true }, canDeactivate: [CanDeactivateGuard] },
                    { path: 'deliver', component: AgentSettingsComponent, data: { title: 'Deliver', header: 'Deliver agent', type: 'deliverAgents', betype: 3, showwarning: true }, canDeactivate: [CanDeactivateGuard] },
                    { path: 'notifyagent', component: AgentSettingsComponent, data: { title: 'Notify agent', header: 'Notify agent', type: 'notifyAgents', betype: 4, showwarning: true }, canDeactivate: [CanDeactivateGuard] },
                    { path: 'receptionawareness', component: ReceptionAwarenessAgentComponent, data: { title: 'Reception awareness', header: 'Reception awareness agent', type: 'receptionAwarenessAgent', betype: 5, showwarning: true }, canDeactivate: [CanDeactivateGuard] },
                    { path: 'pullsend', component: AgentSettingsComponent, data: { title: 'Pull send', header: 'Pull send agent', type: 'pullSendAgents', betype: 7, showwarning: true }, canDeactivate: [CanDeactivateGuard] }
                ]
            }
        ],
        data: { title: 'Settings', icon: 'fa-toggle-on' },
        canActivate: [MustBeAuthorizedGuard],
        canDeactivate: [CanDeactivateGuard]
    }
];
