import { Routes } from '@angular/router';

import { WrapperComponent } from './../common/wrapper.component';
import { MustBeAuthorizedGuard } from './../common/mustbeauthorized.guard';
import { SubmitComponent } from './submit/submit.component';
import { Role } from '../authentication/roles.service';

export const ROUTES: Routes = [
    {
        path: '', component: WrapperComponent, children: [
            { path: 'test', component: SubmitComponent, data: { title: 'Test', icon: 'fa-inbox', weight: 1000, roles: [Role.Admin] } }
        ],
        data: { title: 'test', weight: 1000 },
        canActivate: [MustBeAuthorizedGuard]
    }
];
