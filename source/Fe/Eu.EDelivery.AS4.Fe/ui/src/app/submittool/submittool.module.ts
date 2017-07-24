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
    SubmitToolService
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
        FileUploadModule
    ],
    providers: [
        ...services
    ]
})
export class SubmittoolModule { }
