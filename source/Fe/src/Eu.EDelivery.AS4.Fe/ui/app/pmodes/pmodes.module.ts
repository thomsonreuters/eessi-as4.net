import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { As4ComponentsModule } from './../common/as4components.module';
import { ReceivingPmodeComponent } from './receivingpmode.component';
import { PmodeStore } from './pmode.store';
import { PmodeService } from './pmode.service';

import { ROUTES } from './pmodes.routes';

@NgModule({
    declarations: [
        ReceivingPmodeComponent
    ],
    providers: [
        PmodeStore,
        PmodeService
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
