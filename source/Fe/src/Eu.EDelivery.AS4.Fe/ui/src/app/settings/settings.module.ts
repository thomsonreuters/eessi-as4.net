import { RuntimeService } from './runtime.service';
import { SettingsService } from './settings.service';
import { Component, NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { As4ComponentsModule } from '../common';

import { SettingsComponent } from './settings/settings.component';
import { StepSettingsComponent } from './step/step.component';
import { BaseSettingsComponent } from './base.component';
import { CommonSettingsComponent } from './custom.component';
import { DatabaseSettingsComponent } from './database.component';
import { AgentSettingsComponent } from './agent/agent.component';
import { ReceiverComponent } from './receiver.component';
import { Store } from '../common/store';
import { SettingsStore } from './settings.store';
import { RuntimeStore } from './runtime.store';
import { ROUTES } from './settings.routes';
import { SortablejsModule } from 'angular-sortablejs';
import { ReceptionAwarenessAgentComponent } from './receptionawarenessagent/receptionawarenessagent.component';

@NgModule({
    declarations: [
        SettingsComponent,
        BaseSettingsComponent,
        CommonSettingsComponent,
        DatabaseSettingsComponent,
        AgentSettingsComponent,
        ReceiverComponent,
        StepSettingsComponent,
        ReceptionAwarenessAgentComponent
    ],
    providers: [
        SettingsService,
        RuntimeService,

        SettingsStore,

        RuntimeStore
    ],
    imports: [
        CommonModule,
        As4ComponentsModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule.forChild(ROUTES),
        SortablejsModule
    ],
    exports: [
        SettingsComponent,
        BaseSettingsComponent,
        CommonSettingsComponent,
        DatabaseSettingsComponent,
        AgentSettingsComponent,
        ReceiverComponent
    ]
})
export class SettingsModule {
}
