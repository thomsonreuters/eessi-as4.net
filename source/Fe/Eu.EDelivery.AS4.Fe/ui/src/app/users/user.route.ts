import { Routes } from '@angular/router';

import { MustBeAuthorizedGuard } from './../common/mustbeauthorized.guard';
import { WrapperComponent } from './../common/wrapper.component';
import { UsersComponent } from './users/users.component';
import { CanDeactivateGuard } from './../common/candeactivate.guard';
import { Role } from '../authentication/roles.service';

export const ROUTES: Routes = [
    {
        path: '', component: WrapperComponent, children: [
            { path: 'users', component: UsersComponent, data: { title: 'Users', weight: 9999999999999999999999, roles: [Role.Admin] }, canDeactivate: [CanDeactivateGuard] }
        ],
        canActivate: [MustBeAuthorizedGuard]
    }
];
