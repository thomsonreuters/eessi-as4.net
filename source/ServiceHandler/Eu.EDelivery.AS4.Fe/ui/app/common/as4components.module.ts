import { NgModule } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { BoxComponent } from './box.component';
import { MustBeAuthorizedGuard } from './common.guards';
import { WrapperComponent } from './wrapper.component';
import { SidebarComponent } from './sidebar.component';
import { HeaderComponent } from './header.component';
import { AuthenticationModule } from './../authentication/authentication.module';
import { InputComponent } from './input.component';
import { InfoComponent } from './info.component';
import { TooltipDirective } from './tooltip.directive';

@NgModule({
    declarations: [
        BoxComponent,
        WrapperComponent,
        SidebarComponent,
        HeaderComponent,
        InputComponent,
        InfoComponent,
        TooltipDirective
    ],
    providers: [
        MustBeAuthorizedGuard
    ],
    exports: [
        BoxComponent,
        WrapperComponent,
        SidebarComponent,
        HeaderComponent,
        InputComponent,
        InfoComponent,
        TooltipDirective
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