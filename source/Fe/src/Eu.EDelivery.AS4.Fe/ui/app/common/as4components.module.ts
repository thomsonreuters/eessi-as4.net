import { NgModule } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { BoxComponent } from './box/box.component';
import { MustBeAuthorizedGuard } from './common.guards';
import { WrapperComponent } from './wrapper.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { HeaderComponent } from './header/header.component';
import { AuthenticationModule } from './../authentication/authentication.module';
import { InputComponent } from './input/input.component';
import { InfoComponent } from './info/info.component';
import { TooltipDirective } from './tooltip.directive';
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

@NgModule({
    declarations: [
        BoxComponent,
        WrapperComponent,
        SidebarComponent,
        HeaderComponent,
        InputComponent,
        InfoComponent,
        TooltipDirective,
        RuntimeSettingsComponent,
        CrudButtonsComponent,
        ModalComponent,
        FocusDirective,
        TabComponent,
        TabItemComponent,
        TextDirective,
        SelectDirective
    ],
    providers: [
        MustBeAuthorizedGuard,
        DialogService,
        ModalService,
        // LOGGING_ERROR_HANDLER_PROVIDER
    ],
    exports: [
        BoxComponent,
        WrapperComponent,
        SidebarComponent,
        HeaderComponent,
        InputComponent,
        InfoComponent,
        TooltipDirective,
        RuntimeSettingsComponent,
        CrudButtonsComponent,
        ModalComponent,
        FocusDirective,
        TabComponent,
        TabItemComponent,
        TextDirective,
        SelectDirective
    ],
    imports: [
        AuthenticationModule,
        RouterModule,
        CommonModule,
        ReactiveFormsModule,
        FormsModule
    ]
})
export class As4ComponentsModule {

}
