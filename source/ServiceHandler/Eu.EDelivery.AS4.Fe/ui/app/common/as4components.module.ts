import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { BoxComponent } from './box.component';
import { MustBeAuthorizedGuard } from './common.guards';
import { WrapperComponent } from './wrapper.component';
import { SidebarComponent } from './sidebar.component';
import { HeaderComponent } from './header.component';
import { AuthenticationModule } from './../authentication/authentication.module';

@NgModule({
    declarations: [
        BoxComponent,
        WrapperComponent,
        SidebarComponent,
        HeaderComponent
    ],
    providers: [
        MustBeAuthorizedGuard
    ],
    exports: [
        BoxComponent,
        WrapperComponent,
        SidebarComponent,
        HeaderComponent
    ],
    imports: [
        AuthenticationModule,
        RouterModule,
        CommonModule
    ]
})
export class As4ComponentsModule {

}