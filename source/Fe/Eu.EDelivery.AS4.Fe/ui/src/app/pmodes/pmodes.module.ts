import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { AuthenticationModule } from './../authentication/authentication.module';
import { As4ComponentsModule, errorHandlingServices } from './../common/as4components.module';
import { CrudComponent } from './crud/crud.component';
import { DynamicDiscoveryComponent } from './dynamicdiscoveryprofile/dynamicdiscoveryprofile.component';
import { MessagePackagingComponent } from './messagepackaging/messagepackaging.component';
import { MethodComponent } from './method/method.component';
import { PartyComponent } from './party/party.component';
import { PmodeStore } from './pmode.store';
import { ROUTES } from './pmodes.routes';
import { PmodeSelectComponent } from './pmodeselect/pmodeselect.component';
import { ReceivingPmodeService } from './receivingpmode.service';
import { ReceivingPmodeComponent } from './receivingpmode/receivingpmode.component';
import { RetryReliabilityComponent } from './retryreliability/retryreliability.component';
import { SendingPmodeService } from './sendingpmode.service';
import { SendingPmodeComponent } from './sendingpmode/sendingpmode.component';

const components: any = [
  ReceivingPmodeComponent,
  SendingPmodeComponent,
  PmodeSelectComponent,
  MethodComponent,
  DynamicDiscoveryComponent,
  PartyComponent,
  MessagePackagingComponent,
  CrudComponent,
  RetryReliabilityComponent
];

const services: any = [
  PmodeStore,
  SendingPmodeService,
  ReceivingPmodeService,
  ...errorHandlingServices
];

@NgModule({
  declarations: [...components],
  providers: [...services],
  imports: [
    AuthenticationModule,
    CommonModule,
    As4ComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild(ROUTES)
  ],
  exports: [PmodeSelectComponent]
})
export class PmodesModule {}
