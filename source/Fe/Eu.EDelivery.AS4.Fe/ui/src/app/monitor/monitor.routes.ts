import { ExceptionComponent } from './exception/exception.component';
import { MessageComponent } from './message/message.component';
import { MustBeAuthorizedGuard } from './../common/mustbeauthorized.guard';
import { WrapperComponent } from './../common/wrapper.component';
import { InExceptionComponent } from './inexception/inexception.component';
import { InException } from './../api/Messages/InException';
import { Routes } from '@angular/router';

export const ROUTES: Routes = [
    {
        path: '',
        component: WrapperComponent,
        children: [
            {
                path: 'monitor', children: [
                    { path: '', pathMatch: 'full', redirectTo: 'inexception' },
                    { path: 'messages', component: MessageComponent, data: { title: 'Messages', icon: 'fa-inbox', isAuthCheck: false } },
                    { path: 'exceptions', component: ExceptionComponent, data: { title: 'Exceptions', icon: 'fa-warning', isAuthCheck: false } },
                    { path: 'exceptions/:messageid', component: ExceptionComponent, data: { title: 'Exceptions', nomenu: true, icon: 'fa-warning', isAuthCheck: false } }
                ],
                data: { title: 'Monitor', icon: 'fa-desktop', weight: 100 },
                runGuardsAndResolvers: 'always'
            }
        ],
        canActivate: [MustBeAuthorizedGuard]
    }
];
