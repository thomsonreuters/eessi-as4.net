import { ClipboardModule } from 'ngx-clipboard';
import { Http, RequestOptions, RequestOptionsArgs, Response, XHRBackend, Request } from '@angular/http';
import { NgModule, ModuleWithProviders, ErrorHandler } from '@angular/core';
import { ReactiveFormsModule, FormsModule, FormBuilder } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthHttp, AuthConfig, JwtHelper } from 'angular2-jwt';
import { TextMaskModule } from 'angular2-text-mask';
import { Observable } from 'rxjs/Observable';

import { CanDeactivateGuard } from './candeactivate.guard';
import { SpinnerComponent } from './spinner/spinner.component';
import { ColumnsComponent } from './columns/columns.component';
import { ThumbprintInputComponent } from './thumbprintInput/thumbprintInput.component';
import { ClipboardComponent } from './clipboard/clipboard.component';
import { BoxComponent } from './box/box.component';
import { WrapperComponent } from './wrapper.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { HeaderComponent } from './header/header.component';
import { InputComponent } from './input/input.component';
import { InfoComponent } from './info/info.component';
import { CrudButtonsComponent } from './crudbuttons/crudbuttons.component';
import { ModalComponent } from './modal/modal.component';
import { TabItemDirective } from './tab/tabitem.directive';
import { TabComponent } from './tab/tab.component';

import { RouterService } from './router.service';
import { ToDatePipe } from './../monitor/date.pipe';
import { RolesService } from './../authentication/roles.service';
import { SpinnerService, spinnerHttpServiceFactory } from './spinner/spinner.service';
import { TooltipDirective } from './tooltip.directive';
import { MustBeAuthorizedGuard } from './mustbeauthorized.guard';
import { AuthenticationModule } from './../authentication/authentication.module';
import { DialogService } from './dialog.service';
import { errorHandlerFactory } from './error.handler';
import { ModalService } from './modal/modal.service';
import { TextDirective } from './text.directive';
import { FocusDirective } from './focus.directive';
import { SelectDirective } from './selectdirective';
import { spinnerErrorhandlerDecoratorFactory } from './spinner/spinnerhideerror.handler.factory';
import { DateTimePickerDirective } from './datetimepicker/datetimepicker.directive';
import { ToNumberPipe } from './tonumber.pipe';
import { MultiSelectDirective } from './multiselect/multiselect.directive';
import { ContainsPipe } from './contains.pipe';
import { FormBuilderExtended } from './form.service';
import { FixFormGroupStateDirective } from './fixformgroupstate.directive';

import { Select2Module } from 'ng2-select2';

export function authHttpServiceFactory(http: Http, options: RequestOptions) {
    let result = new AuthHttp(new AuthConfig(), http, options);
    return result;
}

export const errorHandlingServices: any = [
    {
        provide: ErrorHandler,
        useFactory: errorHandlerFactory,
        deps: [DialogService, SpinnerService]
    }
];

const components: any = [
    BoxComponent,
    WrapperComponent,
    SidebarComponent,
    HeaderComponent,
    InputComponent,
    InfoComponent,
    CrudButtonsComponent,
    ModalComponent,
    TabComponent,
    ColumnsComponent,
    SpinnerComponent,
    ThumbprintInputComponent,
    ClipboardComponent
];

const directives: any = [
    FocusDirective,
    TabItemDirective,
    TextDirective,
    SelectDirective,
    TooltipDirective,
    DateTimePickerDirective,
    MultiSelectDirective,
    FixFormGroupStateDirective
];

const pipes: any = [
    ToDatePipe,
    ToNumberPipe,
    ContainsPipe
];

const services: any = [
    MustBeAuthorizedGuard,
    CanDeactivateGuard,
    DialogService,
    ModalService,
    SpinnerService,
    RolesService,
    RouterService,
    FormBuilderExtended,
    {
        provide: Http,
        useFactory: spinnerHttpServiceFactory,
        deps: [XHRBackend, RequestOptions, SpinnerService, DialogService]
    },
    {
        provide: AuthHttp,
        useFactory: authHttpServiceFactory,
        deps: [Http, RequestOptions]
    },
    ...errorHandlingServices
];

@NgModule({
    declarations: [
        ...components,
        ...directives,
        ...pipes
    ],
    providers: [
        ...services
    ],
    exports: [
        BoxComponent,
        WrapperComponent,
        SidebarComponent,
        HeaderComponent,
        InputComponent,
        InfoComponent,
        CrudButtonsComponent,
        ModalComponent,
        FocusDirective,
        TabComponent,
        TabItemDirective,
        TextDirective,
        SelectDirective,
        ColumnsComponent,
        TooltipDirective,
        SpinnerComponent,
        TextMaskModule,
        ThumbprintInputComponent,
        ClipboardComponent,
        DateTimePickerDirective,
        ToDatePipe,
        ToNumberPipe,
        MultiSelectDirective,
        ContainsPipe,
        FixFormGroupStateDirective
    ],
    imports: [
        ClipboardModule,
        RouterModule,
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        TextMaskModule,
        Select2Module,
        AuthenticationModule
    ]
})
export class As4ComponentsModule { }
