import { CanDeactivateGuard } from './../common/candeactivate.guard';
import { Component } from '@angular/core';
import { Routes } from '@angular/router';

import { WrapperComponent } from './../common/wrapper.component';
import { MustBeAuthorizedGuard } from './../common/mustbeauthorized.guard';
import { ReceivingPmodeComponent } from './receivingpmode/receivingpmode.component';
import { SendingPmodeComponent } from './sendingpmode/sendingpmode.component';

export const ROUTES: Routes = [
    {
        path: '', component: WrapperComponent, children: [
            {
                path: 'pmodes', children: [
                    { path: '', pathMatch: 'full', redirectTo: 'receiving', canDeactivate: [CanDeactivateGuard] },
                    { path: 'receiving', component: ReceivingPmodeComponent, data: { title: 'Receiving', mode: 'receiving' }, canDeactivate: [CanDeactivateGuard], canActivate: [MustBeAuthorizedGuard },
                    { path: 'sending', component: SendingPmodeComponent, data: { title: 'Sending', mode: 'sending' }, canDeactivate: [CanDeactivateGuard] },
                ],
                data: { title: 'Pmodes' },
                canActivate: [MustBeAuthorizedGuard]
            }
        ],
        canActivate: [MustBeAuthorizedGuard]
    }
];
