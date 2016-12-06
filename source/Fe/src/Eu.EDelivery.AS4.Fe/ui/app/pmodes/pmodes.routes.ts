import { Component } from '@angular/core';
import { Routes } from '@angular/router';

import { WrapperComponent } from './../common/wrapper.component';
import { ReceivingPmodeComponent } from './receivingpmode.component';
import { MustBeAuthorizedGuard } from './../common/common.guards';

export const ROUTES: Routes = [
    {
        path: '', component: WrapperComponent, children: [
            {
                path: 'pmodes', children: [
                    { path: '', redirectTo: 'receiving' },
                    { path: 'receiving', component: ReceivingPmodeComponent, data: { title: 'Receiving', mode: 'receiving' } },
                    { path: 'sending', component: ReceivingPmodeComponent, data: { title: 'Sending', mode: 'sending' } },
                ],
                data: { title: 'Pmodes' },
                canActivate: [MustBeAuthorizedGuard]
            }
        ]
    }
];
