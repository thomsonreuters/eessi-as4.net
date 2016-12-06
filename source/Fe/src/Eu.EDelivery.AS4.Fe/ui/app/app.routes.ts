import { tokenNotExpired } from 'angular2-jwt';
import { Injectable } from '@angular/core';
import { Routes, RouterModule, CanActivate, Router } from '@angular/router';

import { HomeComponent } from './home';
import { NoContentComponent } from './no-content';

import { WrapperComponent } from './common/wrapper.component';
import { DataResolver } from './app.resolver';
import { MustBeAuthorizedGuard } from './common/common.guards';
import { AgentSettingsComponent } from './settings/agent.component';

export const ROUTES: Routes = [
    { path: '**', component: NoContentComponent }
];
