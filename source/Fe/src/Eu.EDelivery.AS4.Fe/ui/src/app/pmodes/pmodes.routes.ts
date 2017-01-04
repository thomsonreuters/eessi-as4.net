import { Component } from '@angular/core';
import { Routes } from '@angular/router';

import { WrapperComponent } from './../common/wrapper.component';
import { MustBeAuthorizedGuard } from './../common/common.guards';
import { ReceivingPmodeComponent } from './receivingpmode/receivingpmode.component';
import { SendingPmodeComponent } from './sendingpmode/sendingpmode.component';

export const ROUTES: Routes = [
    {
        path: '', component: WrapperComponent, children: [
            {
                path: 'pmodes', children: [
                    { path: '', pathMatch: 'full', redirectTo: 'receiving' },
                    { path: 'receiving', component: ReceivingPmodeComponent, data: { title: 'Receiving', mode: 'receiving' } },
                    { path: 'receiving/:id', component: ReceivingPmodeComponent, data: { title: 'Receiving', mode: 'receiving' } },
                    { path: 'sending', component: SendingPmodeComponent, data: { title: 'Sending', mode: 'sending' } },
                ],
                data: { title: 'Pmodes' },
                canActivate: [MustBeAuthorizedGuard]
            }
        ]
    }
];
