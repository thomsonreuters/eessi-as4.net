import { PartyComponent } from './party.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { As4ComponentsModule } from './../common/as4components.module';
import { ReceivingPmodeComponent } from './receivingpmode.component';
import { PmodeStore } from './pmode.store';
import { PmodeService, pmodeService, ReceivingPmodeService, SendingPmodeService } from './pmode.service';
import { MethodComponent } from './method.component';
import { PmodeSelectComponent } from './pmodeselect.component';

import { ROUTES } from './pmodes.routes';

@NgModule({
    declarations: [
        ReceivingPmodeComponent,
        PmodeSelectComponent,
        MethodComponent,
        PartyComponent
    ],
    providers: [
        PmodeStore,
        PmodeService,
        ReceivingPmodeService,
        SendingPmodeService
    ],
    imports: [
        CommonModule,
        As4ComponentsModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule.forChild(ROUTES)
    ]
})
export class PmodesModule {
}
