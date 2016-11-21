import { tokenNotExpired } from 'angular2-jwt';
import { Injectable } from '@angular/core';
import { Routes, RouterModule, CanActivate, Router } from '@angular/router';

import { HomeComponent } from './home';
import { AboutComponent } from './about';
import { NoContentComponent } from './no-content';

import { DataResolver } from './app.resolver';
import { MustBeAuthorizedGuard } from './common/common.guards';

export const ROUTES: Routes = [
    { path: '', component: HomeComponent, canActivate: [MustBeAuthorizedGuard] },
    { path: '**', component: NoContentComponent },
];