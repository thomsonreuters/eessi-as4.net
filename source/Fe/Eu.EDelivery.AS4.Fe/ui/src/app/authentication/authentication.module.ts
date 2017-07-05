import { FormsModule, FormControlDirective } from '@angular/forms';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { JwtHelper, AuthConfig } from 'angular2-jwt';

import { LoginComponent } from './login/login.component';
import { AuthenticationService, } from './authentication.service';

import { routes } from './authentication.routes';
import { AuthenticationStore } from './authentication.store';
import { TOKENSTORE } from './token';
import { HasAuthDirective } from './hasauth/hasauth.directive';

export function jwtHelperFactory() {
    return new JwtHelper();
}

const components: any = [
    LoginComponent
];

const directives: any = [
    // HasAuthDirective
];

const services: any = [
    { provide: JwtHelper, useFactory: jwtHelperFactory },
    AuthenticationService,
    AuthenticationStore
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        FormsModule
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
        // HasAuthDirective
    ]
})
export class AuthenticationModule { }
