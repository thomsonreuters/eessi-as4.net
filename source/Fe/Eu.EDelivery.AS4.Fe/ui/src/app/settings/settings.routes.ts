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
            { path: 'inbound', component: AgentSettingsComponent, data: { title: 'Inbound', type: 'receiveAgents', icon: 'fa-cloud-download' }, canActivate: [MustBeAuthorizedGuard], canDeactivate: [CanDeactivateGuard] },
            { path: 'outbound', component: AgentSettingsComponent, data: { title: 'Outbound', type: 'submitAgents', icon: 'fa-cloud-upload' }, canActivate: [MustBeAuthorizedGuard], canDeactivate: [CanDeactivateGuard] },
            {
                path: 'settings', children: [
                    { path: '', redirectTo: 'common', pathMatch: 'full', canDeactivate: [CanDeactivateGuard] },
                    { path: 'common', component: SettingsComponent, data: { title: 'Base settings' }, canDeactivate: [CanDeactivateGuard] },
                    {
                        path: 'agents', data: { title: 'Agents' }, children: [
                            { path: '', redirectTo: 'submit', pathMatch: 'full', canDeactivate: [CanDeactivateGuard] },
                            { path: 'submit', component: AgentSettingsComponent, data: { title: 'Submit', header: 'Submit agent', type: 'submitAgents' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'outboundprocessing', component: AgentSettingsComponent, data: { title: 'Outbound processing', header: 'Outbound processing agent', type: 'outboundProcessingAgents' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'send', component: AgentSettingsComponent, data: { title: 'Send', header: 'Send agent', type: 'sendAgents' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'receive', component: AgentSettingsComponent, data: { title: 'Receive', header: 'Receive agent', type: 'receiveAgents' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'deliver', component: AgentSettingsComponent, data: { title: 'Deliver', header: 'Deliver agent', type: 'deliverAgents' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'notify', component: AgentSettingsComponent, data: { title: 'Notify', header: 'Notify agent', type: 'notifyAgents' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'notifyconsumer', component: AgentSettingsComponent, data: { title: 'Notify consumer', header: 'Notify consumer agent', type: 'notifyConsumerAgents' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'notifyproducer', component: AgentSettingsComponent, data: { title: 'Notify producer', header: 'Notify producer agent', type: 'notifyProducerAgents' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'receptionawareness', component: ReceptionAwarenessAgentComponent, data: { title: 'Reception', header: 'Reception awareness agent', type: 'receptionAwarenessAgent' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'pullreceive', component: AgentSettingsComponent, data: { title: 'Pull receive', header: 'Pull receive agent', type: 'pullReceiveAgents' }, canDeactivate: [CanDeactivateGuard] },
                            { path: 'pullsend', component: AgentSettingsComponent, data: { title: 'Pull send', header: 'Pull send agent', type: 'pullSendAgents' }, canDeactivate: [CanDeactivateGuard] }
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
