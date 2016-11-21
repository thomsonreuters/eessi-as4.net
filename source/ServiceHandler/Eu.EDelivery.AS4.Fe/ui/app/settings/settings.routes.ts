import { Component } from '@angular/core';
import { Routes } from '@angular/router';

import { SettingsComponent } from './settings.component';

export const ROUTES: Routes = [
    {
        path: 'settings', children: [
            { path: '', component: SettingsComponent }
        ]
    }
]