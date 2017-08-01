
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { ClipboardModule } from 'ngx-clipboard';

import { As4ComponentsModule } from './../common/as4components.module';
import { AuthenticationModule } from './../authentication/authentication.module';

import { ROUTES } from './monitor.routes';
import { FilterComponent } from './filter/filter.component';
import { InExceptionStore } from './inexception/inexception.store';
import { PagerComponent } from './pager/pager.component';
import { ToNumberArrayPipe } from './numbertoarray.pipe';
import { RelatedMessagesComponent } from './relatedmessages/relatedmessages.component';
import { Select2Module } from 'ng2-select2';
import { DownloadMessageBodyComponent } from './downloadmessagebody/downloadmessagebody.component';
import { ExceptionDetailComponent } from './exceptiondetail/exceptiondetail.component';
import { ToDirectionPipe } from './todirection.pipe';
import { ExceptionService } from './exception/exception.service';
import { ExceptionStore } from './exception/exception.store';
import { ExceptionComponent } from './exception/exception.component';
import { MessageService } from './message/message.service';
import { ErrorMessageComponent } from './errormessage/errormessage.component';
import { MessageComponent } from './message/message.component';
import { MessageStore } from './message/message.store';
import { SortDirective } from './sort/sort.directive';

const components: any = [
    ExceptionComponent,
    FilterComponent,
    PagerComponent,
    MessageComponent,
    ErrorMessageComponent,
    RelatedMessagesComponent,
    DownloadMessageBodyComponent,
    ExceptionDetailComponent
];

const directives: any = [
    SortDirective
];

const pipes: any = [
    ToNumberArrayPipe,
    ToDirectionPipe
];

@NgModule({
    declarations: [
        ...components,
        ...directives,
        ...pipes
    ],
    imports: [
        AuthenticationModule,
        As4ComponentsModule,
        FormsModule,
        ReactiveFormsModule,
        CommonModule,
        ClipboardModule,
        RouterModule.forChild(ROUTES),
        Select2Module
    ],
    providers: [
        MessageStore,
        MessageService,
        ExceptionStore,
        ExceptionService
    ]
})
export class MonitorModule { }
