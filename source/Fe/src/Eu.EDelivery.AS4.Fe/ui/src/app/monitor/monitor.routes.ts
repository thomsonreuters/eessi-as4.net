import { MessageComponent } from './message/message.component';
import { MustBeAuthorizedGuard } from './../common/mustbeauthorized.guard';
import { WrapperComponent } from './../common/wrapper.component';
import { InExceptionComponent } from './inexception/inexception.component';
import { InException } from './../api/Messages/InException';
import { Routes } from '@angular/router';

export const ROUTES: Routes = [
    {
        path: '', component: WrapperComponent, children: [
            {
                path: 'monitor', children: [
                    { path: '', pathMatch: 'full', redirectTo: 'inexception' },
                    { path: 'messages', component: MessageComponent, data: { title: 'Message' } },
                ],
                data: { title: 'Monitor', weight: 100 }
            }
        ],
        canActivate: [MustBeAuthorizedGuard]
    }
];
