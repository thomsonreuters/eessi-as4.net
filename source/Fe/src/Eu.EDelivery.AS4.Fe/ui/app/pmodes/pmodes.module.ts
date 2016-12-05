import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { As4ComponentsModule } from './../common/as4components.module';
import { PmodeComponent } from './pmode.component';

import { ROUTES } from './pmodes.routes';

@NgModule({
    declarations: [
        PmodeComponent
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