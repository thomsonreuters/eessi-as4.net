import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';

import { As4ComponentsModule } from './../common/as4components.module';
import { RuntimeSettingComponent } from './runtimesetting/runtimesetting.component';
import { RuntimeSettingsComponent } from './runtimesettings/runtimesettings.component';
import { GetValuePipe, GetKeysPipe } from './runtimesettings/getvalue.pipe';
import { PmodesModule } from './../pmodes/pmodes.module';
import { AuthenticationModule } from './../authentication/authentication.module';

@NgModule({
    declarations: [
        RuntimeSettingComponent,
        RuntimeSettingsComponent,

        GetValuePipe,
        GetKeysPipe
    ],
    imports: [
        FormsModule,
        ReactiveFormsModule,
        CommonModule,
        As4ComponentsModule,
        PmodesModule,
        AuthenticationModule
    ],
    exports: [
        RuntimeSettingComponent,
        RuntimeSettingsComponent,

        GetValuePipe,
        GetKeysPipe
    ]
})
export class RuntimeModule { }
