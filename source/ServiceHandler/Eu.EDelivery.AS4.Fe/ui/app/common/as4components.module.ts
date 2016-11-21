import { NgModule } from '@angular/core';

import { BoxComponent } from './box.component';
import { MustBeAuthorizedGuard } from './common.guards';

@NgModule({
    declarations: [
        BoxComponent
    ],
    providers: [
        MustBeAuthorizedGuard
    ],
    exports: [
        BoxComponent
    ]
})
export class As4ComponentsModule {

}