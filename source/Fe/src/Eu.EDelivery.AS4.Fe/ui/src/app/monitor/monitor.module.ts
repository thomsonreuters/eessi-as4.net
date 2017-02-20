import { ExceptionService } from './exception/exception.service';
import { ExceptionStore } from './exception/exception.store';
import { ExceptionComponent } from './exception/exception.component';
import { MessageService } from './message/message.service';
import { ErrorMessageComponent } from './errormessage/errormessage.component';
import { MessageComponent } from './message/message.component';
import { MessageStore } from './message/message.store';
import { SortDirective } from './sort/sort.directive';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { ClipboardModule } from 'ngx-clipboard';

import { As4ComponentsModule } from './../common/as4components.module';
import { AuthenticationModule } from './../authentication/authentication.module';

import { ROUTES } from './monitor.routes';
import { FilterComponent } from './filter/filter.component';
import { InExceptionStore } from './inexception/inexception.store';
import { PagerComponent } from './pager/pager.component';
import { ToNumberArrayPipe } from './numbertoarray.pipe';

@NgModule({
    declarations: [
        ExceptionComponent,
        FilterComponent,
        ToNumberArrayPipe,
        PagerComponent,
        SortDirective,
        MessageComponent,
        ErrorMessageComponent
    ],
    imports: [
        AuthenticationModule,
        As4ComponentsModule,
        FormsModule,
        CommonModule,
        ClipboardModule,
        RouterModule.forChild(ROUTES)
    ],
    providers: [
        MessageStore,
        MessageService,
        ExceptionStore,
        ExceptionService
    ]
})
export class MonitorModule {

}
