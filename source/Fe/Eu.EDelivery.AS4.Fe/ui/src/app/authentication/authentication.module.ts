import { FormsModule, FormControlDirective, ReactiveFormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { JwtHelper, AuthConfig } from 'angular2-jwt';

import { LoginComponent } from './login/login.component';
import { AuthenticationService, } from './authentication.service';

import { routes } from './authentication.routes';
import { AuthenticationStore } from './authentication.store';
import { TOKENSTORE } from './token';
import { HasAuthDirective } from './hasauth/hasauth.directive';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { SetupGuard } from './../setup/setup.guard';
import { LogoutService } from './logout.service';

export function jwtHelperFactory() {
    return new JwtHelper();
}

const components: any = [
    LoginComponent,
    UnauthorizedComponent
];

const directives: any = [
    HasAuthDirective
];

const services: any = [
    { provide: JwtHelper, useFactory: jwtHelperFactory },
    AuthenticationService,
    AuthenticationStore,
    LogoutService,

    SetupGuard
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        FormsModule,
        ReactiveFormsModule
    ],
    declarations: [
        ...components,
        ...directives
    ],
    providers: [
        ...services
    ],
    exports: [
        LoginComponent,
        HasAuthDirective
    ]
})
export class AuthenticationModule { }
