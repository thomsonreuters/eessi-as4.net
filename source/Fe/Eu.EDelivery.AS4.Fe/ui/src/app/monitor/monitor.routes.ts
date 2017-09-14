import { Routes } from '@angular/router';

import { ExceptionComponent } from './exception/exception.component';
import { MessageComponent } from './message/message.component';
import { MustBeAuthorizedGuard } from './../common/mustbeauthorized.guard';
import { WrapperComponent } from './../common/wrapper.component';
import { InExceptionComponent } from './inexception/inexception.component';
import { InException } from './../api/Messages/InException';
import { ExceptionDetailComponent } from './exceptiondetail/exceptiondetail.component';

export const ROUTES: Routes = [
    {
        path: 'monitor',
        component: WrapperComponent,
        children: [
            { path: '', pathMatch: 'full', redirectTo: 'inexception' },
            { path: 'messages', component: MessageComponent, data: { title: 'Messages', icon: 'fa-inbox', isAuthCheck: false } },
            { path: 'exceptions', component: ExceptionComponent, data: { title: 'Exceptions', icon: 'fa-warning', isAuthCheck: false } },
            { path: 'exceptions/:direction/:messageid', component: ExceptionDetailComponent, data: { title: 'Exception detail', nomenu: true, icon: 'fa-warning', isAuthCheck: false } }
        ],
        data: { title: 'Monitor', icon: 'fa-desktop', weight: 100 },
        runGuardsAndResolvers: 'always',
        canActivate: [MustBeAuthorizedGuard]
    }
];
