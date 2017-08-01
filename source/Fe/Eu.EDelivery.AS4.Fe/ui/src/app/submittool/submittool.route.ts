import { Routes } from '@angular/router';

import { WrapperComponent } from './../common/wrapper.component';
import { MustBeAuthorizedGuard } from './../common/mustbeauthorized.guard';
import { SubmitComponent } from './submit/submit.component';

export const ROUTES: Routes = [
    // { path: '**', component: NoContentComponent }
    {
        path: '', component: WrapperComponent, children: [
            { path: 'test', component: SubmitComponent, data: { title: 'Test', icon: 'fa-inbox', weight: 1000, isAuthCheck: false } }
        ],
        data: { title: 'test', weight: 1000 },
        canActivate: [MustBeAuthorizedGuard]
    }
];
