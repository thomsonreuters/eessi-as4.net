import { ThumbprintInputComponent } from './thumbprintInput/thumbprintInput.component';
import { Http, RequestOptions, RequestOptionsArgs, Response, XHRBackend, Request } from '@angular/http';
import { SpinnerService, spinnerHttpServiceFactory } from './spinner/spinner.service';
import { SpinnerComponent } from './spinner/spinner.component';
import { TooltipDirective } from './tooltip.directive';
import { ColumnsComponent } from './columns/columns.component';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthHttp, AuthConfig } from 'angular2-jwt';
import { TextMaskModule } from 'angular2-text-mask';
import { Observable } from 'rxjs/Observable';

import { BoxComponent } from './box/box.component';
import { MustBeAuthorizedGuard } from './common.guards';
import { WrapperComponent } from './wrapper.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { HeaderComponent } from './header/header.component';
import { AuthenticationModule } from './../authentication/authentication.module';
import { InputComponent } from './input/input.component';
import { InfoComponent } from './info/info.component';
import { DialogService } from './dialog.service';
import { LOGGING_ERROR_HANDLER_PROVIDER } from './error.handler';
import { RuntimeSettingsComponent } from './runtimesettings/runtimesettings.component';
import { CrudButtonsComponent } from './crudbuttons/crudbuttons.component';
import { ModalService } from './modal/modal.service';
import { ModalComponent } from './modal/modal.component';
import { TextDirective } from './text.directive';
import { TabItemComponent } from './tab/tabitem.component';
import { TabComponent } from './tab/tab.component';
import { FocusDirective } from './focus.directive';
import { SelectDirective } from './selectdirective';

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
        TabItemComponent,
        TextDirective,
        SelectDirective,
        ColumnsComponent,
        TooltipDirective,
        SpinnerComponent,
        ThumbprintInputComponent
    ],
    providers: [
        MustBeAuthorizedGuard,
        DialogService,
        ModalService,
        SpinnerService,
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
        TabItemComponent,
        TextDirective,
        SelectDirective,
        ColumnsComponent,
        TooltipDirective,
        SpinnerComponent,
        TextMaskModule,
        ThumbprintInputComponent
    ],
    imports: [
        AuthenticationModule,
        RouterModule,
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        TextMaskModule
    ]
})
export class As4ComponentsModule {

}
