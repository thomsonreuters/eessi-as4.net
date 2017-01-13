import { ThumbprintInputComponent } from './thumbprintInput/thumbprintInput.component';
import { Http, RequestOptions, RequestOptionsArgs, Response, XHRBackend } from '@angular/http';
import { SpinnerService } from './spinner/spinner.service';
import { SpinnerComponent } from './spinner/spinner.component';
import { TooltipDirective } from './tooltip.directive';
import { ColumnsComponent } from './columns/columns.component';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthHttp, AuthConfig } from 'angular2-jwt';
import { TextMaskModule } from 'angular2-text-mask';

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

import { Observable } from 'rxjs/Observable';

export class CustomHttp extends Http {
    constructor(backend: XHRBackend, options: RequestOptions, private spinnerService: SpinnerService) {
        super(backend, options);
    }
    get(url: string, options?: RequestOptionsArgs): Observable<Response> {
        this.spinnerService.show();
        let res = super.get(url, options);
        return res;
    }
}

export function authHttpServiceFactory(http: Http, options: RequestOptions) {
    return new AuthHttp(new AuthConfig(), http, options);
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
            provide: AuthHttp,
            useFactory: authHttpServiceFactory,
            deps: [Http, RequestOptions]
        }
        // AUTH_PROVIDERS,
        // {
        //     provide: Http, useFactory: (backend, requestOptions, spinnerService) => {
        //         return new CustomHttp(backend, requestOptions, spinnerService);
        //     }, deps: [XHRBackend, RequestOptions, SpinnerService]
        // },
        // LOGGING_ERROR_HANDLER_PROVIDER
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
    static forRoot(): ModuleWithProviders {
        return {
            ngModule: As4ComponentsModule,
            providers: [SpinnerService, SpinnerComponent]
        };
    }
}
