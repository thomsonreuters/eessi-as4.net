import { CommonModule } from '@angular/common';
import { ErrorHandler, Injector, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Http, RequestOptions, XHRBackend } from '@angular/http';
import { RouterModule } from '@angular/router';
import { AuthConfig, AuthHttp } from 'angular2-jwt';
import { TextMaskModule } from 'angular2-text-mask';
import { Select2Module } from 'ng2-select2';
import { ClipboardModule } from 'ngx-clipboard';

import { AuthenticationModule } from './../authentication/authentication.module';
import { RolesService } from './../authentication/roles.service';
import { ToDatePipe } from './../monitor/date.pipe';
import { BoxComponent } from './box/box.component';
import { CanDeactivateGuard } from './candeactivate.guard';
import { ClipboardComponent } from './clipboard/clipboard.component';
import { ColumnsComponent } from './columns/columns.component';
import { ContainsPipe } from './contains.pipe';
import { CrudButtonsComponent } from './crudbuttons/crudbuttons.component';
import { DateTimePickerDirective } from './datetimepicker/datetimepicker.directive';
import { DialogService } from './dialog.service';
import { errorHandlerFactory } from './error.handler';
import { FileSelectComponent } from './fileselect/fileselect.component';
import { FixFormGroupStateDirective } from './fixformgroupstate.directive';
import { FocusDirective, TabIndexDirective } from './focus.directive';
import { FormBuilderExtended } from './form.service';
import { FormErrorComponent } from './formerror/formerror.component';
import { GetItemTypePropertyPipe, GetTypePipe } from './getitemtypeproperty.pipe';
import { HeaderComponent } from './header/header.component';
import { InfoComponent } from './info/info.component';
import { InputComponent } from './input/input.component';
import { ModalComponent } from './modal/modal.component';
import { ModalService } from './modal/modal.service';
import { MultiSelectDirective, OptionDirective } from './multiselect/multiselect.directive';
import { MustBeAuthorizedGuard } from './mustbeauthorized.guard';
import { PasswordComponent } from './password/password.component';
import { RouterService } from './router.service';
import { RuntimetoolTipDirective } from './runtimetooltip.directive';
import { Select2Component } from './select2/select2.component';
import { SelectDirective } from './selectdirective';
import { SidebarComponent } from './sidebar/sidebar.component';
import { CustomAuthNoSpinnerHttp, CustomHttp } from './spinner/customhttp';
import { SpinnerComponent } from './spinner/spinner.component';
import { spinnerHttpServiceFactory, SpinnerService } from './spinner/spinner.service';
import { TabComponent } from './tab/tab.component';
import { TabItemDirective } from './tab/tabitem.directive';
import { TextDirective } from './text.directive';
import { ThumbprintInputComponent } from './thumbprintInput/thumbprintInput.component';
import { ThumbprintValidatorDirective } from './thumbprintInput/validator';
import { TimeInputComponent } from './timeinput/timeinput.component';
import { ToNumberPipe } from './tonumber.pipe';
import { TooltipDirective } from './tooltip.directive';
import { WrapperComponent } from './wrapper.component';

export function authHttpServiceFactory(
    options: RequestOptions,
    backend: XHRBackend,
    spinnerService: SpinnerService,
    dialogService: DialogService,
    injector: Injector
) {
    let result = new AuthHttp(
        new AuthConfig(),
        new CustomHttp(backend, options, spinnerService, dialogService, injector),
        options
    );
    return result;
}

export function authHttpNoSpinnerServiceFactory(
    options: RequestOptions,
    backend: XHRBackend,
    spinnerService: SpinnerService,
    dialogService: DialogService,
    injector: Injector
) {
    let customHttp = new CustomHttp(backend, options, spinnerService, dialogService, injector);
    customHttp.noSpinner = true;
    let result = new AuthHttp(new AuthConfig(), customHttp, options);
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
    ClipboardComponent,
    TimeInputComponent,
    Select2Component,
    PasswordComponent,
    FileSelectComponent,
    FormErrorComponent
];

const directives: any = [
    FocusDirective,
    TabItemDirective,
    TextDirective,
    SelectDirective,
    TooltipDirective,
    DateTimePickerDirective,
    MultiSelectDirective,
    FixFormGroupStateDirective,
    OptionDirective,
    TabIndexDirective,
    RuntimetoolTipDirective,
    ThumbprintValidatorDirective
];

const pipes: any = [ToDatePipe, ToNumberPipe, ContainsPipe, GetItemTypePropertyPipe, GetTypePipe];

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
        deps: [XHRBackend, RequestOptions, SpinnerService, DialogService, Injector]
    },
    {
        provide: AuthHttp,
        useFactory: authHttpServiceFactory,
        deps: [RequestOptions, XHRBackend, SpinnerService, DialogService, Injector]
    },
    {
        provide: CustomAuthNoSpinnerHttp,
        useFactory: authHttpNoSpinnerServiceFactory,
        deps: [RequestOptions, XHRBackend, SpinnerService, DialogService, Injector]
    },
    ...errorHandlingServices
];

@NgModule({
    declarations: [...components, ...directives, ...pipes],
    providers: [...services],
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
        FixFormGroupStateDirective,
        TimeInputComponent,
        OptionDirective,
        TabIndexDirective,
        GetItemTypePropertyPipe,
        GetTypePipe,
        RuntimetoolTipDirective,
        Select2Component,
        ThumbprintValidatorDirective,
        PasswordComponent,
        FileSelectComponent,
        FormErrorComponent
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
export class As4ComponentsModule {}
