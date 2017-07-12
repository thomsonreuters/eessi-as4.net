import { AuthenticationModule } from './../authentication/authentication.module';
import { CrudComponent } from './crud/crud.component';
import { ReceivingPmode } from './../api/ReceivingPmode';
import { MessagePackagingComponent } from './messagepackaging/messagepackaging.component';
import { SendingPmodeComponent } from './sendingpmode/sendingpmode.component';
import { PartyComponent } from './party/party.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { As4ComponentsModule, errorHandlingServices } from './../common/as4components.module';
import { ReceivingPmodeComponent } from './receivingpmode/receivingpmode.component';
import { PmodeStore } from './pmode.store';
import { SendingPmodeService } from './sendingpmode.service';
import { ReceivingPmodeService } from './receivingpmode.service';
import { MethodComponent } from './method/method.component';
import { PmodeSelectComponent } from './pmodeselect/pmodeselect.component';

import { ROUTES } from './pmodes.routes';

import { AuthHttp } from 'angular2-jwt';
import { Http, RequestOptions } from '@angular/http';
import { authHttpServiceFactory } from '../common/as4components.module';

const components: any = [
    ReceivingPmodeComponent,
    SendingPmodeComponent,
    PmodeSelectComponent,
    MethodComponent,
    PartyComponent,
    MessagePackagingComponent,
    CrudComponent
];

const services: any = [
    PmodeStore,
    SendingPmodeService,
    ReceivingPmodeService,
    ...errorHandlingServices
];

@NgModule({
    declarations: [
        ...components
    ],
    providers: [
        ...services
    ],
    imports: [
        AuthenticationModule,
        CommonModule,
        As4ComponentsModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule.forChild(ROUTES)
    ],
    exports: [
        PmodeSelectComponent
    ]
})
export class PmodesModule {
}
