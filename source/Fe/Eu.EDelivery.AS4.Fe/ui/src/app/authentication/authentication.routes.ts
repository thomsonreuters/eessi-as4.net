import { Routes } from '@angular/router';

import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { SetupGuard } from './../setup/setup.guard';
import { LoginComponent } from './login/login.component';

export const routes: Routes = [
    { path: 'login', component: LoginComponent, data: { isAuthCheck: false }, canActivate: [SetupGuard] },
    { path: 'unauthorized', component: UnauthorizedComponent }
];
