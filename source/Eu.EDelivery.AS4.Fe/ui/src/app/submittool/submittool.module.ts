import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FileUploadModule } from 'ng2-file-upload';

import { As4ComponentsModule } from './../common/as4components.module';
import { PmodesModule } from './../pmodes/pmodes.module';

import { ROUTES } from './submitTool.route';

import { SubmitComponent } from './submit/submit.component';
import { ProgressComponent } from './progress/progress.component';

import { SubmitToolService } from './submittool.service';
import { FileSizePipe } from './fileSize.pipe';
import { MarkAsTouchedDirective } from './markastouched.directive';
import { AsXmlPipe } from './asxml.pipe';
import { SignalRModule, SignalRConfiguration } from 'ng2-signalr';
import { SignalrService } from './signalr.service';

import '../../../node_modules/signalr/jquery.signalR.js';

export function createSignalRConfig(): SignalRConfiguration {
    const c = new SignalRConfiguration();
    c.hubName = 'SubmitToolMessageHub';
    c.logging = true;
    return c;
}

const components: any[] = [
    SubmitComponent,
    ProgressComponent,
    FileSizePipe,
    AsXmlPipe
];

const directives: any[] = [
    MarkAsTouchedDirective
];

const services: any[] = [
    SubmitToolService,
    SignalrService
];

@NgModule({
    declarations: [
        ...components,
        ...directives
    ],
    imports: [
        As4ComponentsModule,
        CommonModule,
        RouterModule.forChild(ROUTES),
        PmodesModule,
        FormsModule,
        FileUploadModule,
        SignalRModule.forRoot(createSignalRConfig)
    ],
    providers: [
        ...services
    ]
})
export class SubmittoolModule {
    constructor(private sevice: SignalrService) { }
}
