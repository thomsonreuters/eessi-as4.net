import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { LoginComponent } from './login.component';
import { AuthenticationService, AuthenticationStore } from './authentication.service';

import { routes } from './authentication.routes';

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
