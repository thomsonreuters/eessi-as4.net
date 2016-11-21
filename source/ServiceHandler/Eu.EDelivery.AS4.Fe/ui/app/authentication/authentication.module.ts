import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { LoginComponent } from './login.component';
import { AuthenticationService, AuthenticationStore } from './authentication.service';

export const routes: Routes = [
    { path: 'login', component: LoginComponent }
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        FormsModule
    ],
    declarations: [
        LoginComponent
    ],
    providers: [
        AuthenticationService,
        AuthenticationStore
    ],
    exports: [
        LoginComponent
    ]
})
export class AuthenticationModule {

}