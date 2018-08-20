import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';

import { SetupService } from './setup.service';
import { SetupComponent } from './setup/setup.component';
import { SetupGuard } from './setup.guard';
import { ROUTES } from './setup.routes';
import { As4ComponentsModule, errorHandlingServices } from './../common/as4components.module';

@NgModule({
    imports: [
        RouterModule.forRoot(ROUTES),
        ReactiveFormsModule,
        CommonModule,
        As4ComponentsModule
    ],
    exports: [],
    declarations: [SetupComponent],
    providers: [SetupService, SetupGuard, ...errorHandlingServices]
})
export class SetupModule { }
