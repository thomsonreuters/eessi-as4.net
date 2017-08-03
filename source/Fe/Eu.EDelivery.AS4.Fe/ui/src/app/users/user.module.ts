import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';

import { Component } from './name.component';
import { UsersComponent } from './users/users.component';
import { UsersService } from './users.service';
import { As4ComponentsModule } from './../common/as4components.module';
import { ROUTES } from './user.route';

@NgModule({
    imports: [
        RouterModule.forChild(ROUTES),
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        As4ComponentsModule
    ],
    exports: [],
    declarations: [
        UsersComponent
    ],
    providers: [
        UsersService
    ],
})
export class UserModule { }
