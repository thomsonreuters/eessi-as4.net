import { SettingsService } from './settings.service';
import { Component, NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { As4ComponentsModule } from '../common';

import { SettingsComponent } from './settings.component';
import { BaseSettingsComponent } from './base.component';
import { CommonSettingsComponent } from './custom.component';
import { DatabaseSettingsComponent } from './database.component';
import { AgentSettingsComponent } from './agent.component';
import { ReceiverComponent } from './receiver.component';
import { DecoratorComponent } from './decorator.component';
import { SettingsStore, StoreHelper } from './settings.store';

@NgModule({
    declarations: [
        SettingsComponent,
        BaseSettingsComponent,
        CommonSettingsComponent,
        DatabaseSettingsComponent,
        AgentSettingsComponent,
        ReceiverComponent,
        DecoratorComponent
    ],
    providers: [
        SettingsStore,
        SettingsService,
        StoreHelper
    ],
    imports: [
        CommonModule,
        As4ComponentsModule,
        FormsModule
    ],
    exports: [
        SettingsComponent,
        BaseSettingsComponent,
        CommonSettingsComponent,
        DatabaseSettingsComponent,
        AgentSettingsComponent,
        ReceiverComponent,
        DecoratorComponent
    ]
})
export class SettingsModule {
}
