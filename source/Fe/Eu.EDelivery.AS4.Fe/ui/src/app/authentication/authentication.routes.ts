import { Routes } from '@angular/router';

import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { LoginComponent } from './login/login.component';

export const routes: Routes = [
    { path: 'login', component: LoginComponent, data: { isAuthCheck: false } },
    { path: 'unauthorized', component: UnauthorizedComponent }
];
