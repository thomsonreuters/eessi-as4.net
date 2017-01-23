import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { LoginComponent } from './login/login.component';
import { AuthenticationService, } from './authentication.service';

import { routes } from './authentication.routes';
import { AuthenticationStore } from './authentication.store';

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
