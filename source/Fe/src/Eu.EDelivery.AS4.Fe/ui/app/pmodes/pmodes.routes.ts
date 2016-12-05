import { PmodeComponent } from './pmode.component';
import { Component } from '@angular/core';
import { Routes } from '@angular/router';

import { WrapperComponent } from './../common/wrapper.component';
import { MustBeAuthorizedGuard } from './../common/common.guards';

export const ROUTES: Routes = [
    {
        path: 'pmodes', component: WrapperComponent, children: [
            { path: '', redirectTo: 'common' },
            { path: 'common', component: PmodeComponent, data: { title: 'pmode' } }
        ],
        data: { title: 'Pmodes' },
        canActivate: [MustBeAuthorizedGuard]
    }
];
