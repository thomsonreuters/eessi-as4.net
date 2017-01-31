import { ClipboardModule } from 'ngx-clipboard';
import { ClipboardComponent } from './clipboard/clipboard.component';
import { RolesService } from './../authentication/roles.service';
import { CanDeactivateGuard } from './candeactivate.guard';
import { ThumbprintInputComponent } from './thumbprintInput/thumbprintInput.component';
import { Http, RequestOptions, RequestOptionsArgs, Response, XHRBackend, Request } from '@angular/http';
import { SpinnerService, spinnerHttpServiceFactory } from './spinner/spinner.service';
import { SpinnerComponent } from './spinner/spinner.component';
import { TooltipDirective } from './tooltip.directive';
import { ColumnsComponent } from './columns/columns.component';
import { NgModule, ModuleWithProviders, ErrorHandler } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthHttp, AuthConfig, JwtHelper } from 'angular2-jwt';
import { TextMaskModule } from 'angular2-text-mask';
import { Observable } from 'rxjs/Observable';

import { BoxComponent } from './box/box.component';
import { MustBeAuthorizedGuard } from './mustbeauthorized.guard';
import { WrapperComponent } from './wrapper.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { HeaderComponent } from './header/header.component';
import { AuthenticationModule } from './../authentication/authentication.module';
import { InputComponent } from './input/input.component';
import { InfoComponent } from './info/info.component';
import { DialogService } from './dialog.service';
import { errorHandlerFactory } from './error.handler';
import { RuntimeSettingsComponent } from './runtimesettings/runtimesettings.component';
import { CrudButtonsComponent } from './crudbuttons/crudbuttons.component';
import { ModalService } from './modal/modal.service';
import { ModalComponent } from './modal/modal.component';
import { TextDirective } from './text.directive';
import { TabItemDirective } from './tab/tabitem.directive';
import { TabComponent } from './tab/tab.component';
import { FocusDirective } from './focus.directive';
import { SelectDirective } from './selectdirective';
import { spinnerErrorhandlerDecoratorFactory } from './spinner/spinnerhideerror.handler.factory';
import { DateTimePickerDirective } from './datetimepicker/datetimepicker.directive';

export function authHttpServiceFactory(http: Http, options: RequestOptions) {
    let result = new AuthHttp(new AuthConfig(), http, options);
    return result;
}

@NgModule({
    declarations: [
        BoxComponent,
        WrapperComponent,
        SidebarComponent,
        HeaderComponent,
        InputComponent,
        InfoComponent,
        RuntimeSettingsComponent,
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
        ThumbprintInputComponent,
        ClipboardComponent,
        DateTimePickerDirective
    ],
    providers: [
        MustBeAuthorizedGuard,
        CanDeactivateGuard,
        DialogService,
        ModalService,
        SpinnerService,
        RolesService,
        ClipboardModule,
        // {
        //     provide: ErrorHandler,
        //     useFactory: errorHandlerFactory,
        //     deps: [DialogService, SpinnerService]
        // },
        // {
        //     provide: ErrorHandler,
        //     useFactory: spinnerErrorhandlerDecoratorFactory,
        //     deps: [SpinnerService]
        // },
        {
            provide: Http,
            useFactory: spinnerHttpServiceFactory,
            deps: [XHRBackend, RequestOptions, SpinnerService]
        },
        {
            provide: AuthHttp,
            useFactory: authHttpServiceFactory,
            deps: [Http, RequestOptions]
        }
    ],
    exports: [
        BoxComponent,
        WrapperComponent,
        SidebarComponent,
        HeaderComponent,
        InputComponent,
        InfoComponent,
        RuntimeSettingsComponent,
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
        DateTimePickerDirective
    ],
    imports: [
        AuthenticationModule,
        RouterModule,
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        TextMaskModule,
        ClipboardModule
    ]
})
export class As4ComponentsModule {

}
