import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SortablejsModule } from 'angular-sortablejs';

import { As4ComponentsModule } from '../common';
import { AuthenticationModule } from './../authentication/authentication.module';
import { RuntimeModule } from './../runtime/runtime.module';
import { AgentSettingsComponent } from './agent/agent.component';
import { AuthorizationMapComponent } from './authorizationmap/authorizationmap.component';
import { AuthorizationMapService } from './authorizationmap/authorizationmapservice';
import { BaseSettingsComponent } from './base.component';
import { CommonSettingsComponent } from './commonsettings.component';
import { DatabaseSettingsComponent } from './database.component';
import { PortalSettingsComponent } from './portalsettings/portalsettings.component';
import { ReceiverComponent } from './receiver.component';
import { RuntimeService } from './runtime.service';
import { RuntimeStore } from './runtime.store';
import { ROUTES } from './settings.routes';
import { SettingsService } from './settings.service';
import { SettingsStore } from './settings.store';
import { SettingsComponent } from './settings/settings.component';
import { SmpConfigurationComponent } from './smpconfiguration/smpconfiguration.component';
import { SmpConfigurationService } from './smpconfiguration/smpconfiguration.service';
import { SmpConfigurationDetailComponent } from './smpconfiguration/smpconfigurationdetail.component';
import { StepSettingsComponent } from './step/step.component';
import { TransformerComponent } from './transformer.component';

const components: any = [
  SettingsComponent,
  BaseSettingsComponent,
  CommonSettingsComponent,
  DatabaseSettingsComponent,
  AgentSettingsComponent,
  ReceiverComponent,
  TransformerComponent,
  StepSettingsComponent,
  PortalSettingsComponent,
  AuthorizationMapComponent,
  SmpConfigurationComponent,
  SmpConfigurationDetailComponent
];

const services: any = [
  SettingsService,
  RuntimeService,
  SettingsStore,
  RuntimeStore,
  AuthorizationMapService,
  SmpConfigurationService
];

@NgModule({
  entryComponents: [SmpConfigurationDetailComponent],
  declarations: [...components],
  providers: [...services],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild(ROUTES),
    SortablejsModule,
    AuthenticationModule,
    As4ComponentsModule,
    RuntimeModule
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
export class SettingsModule {}
