import { AuthenticationStore } from './authentication/authentication.store';
import { AuthConfig } from 'angular2-jwt';
import { ClipboardModule } from 'ngx-clipboard';
import { CommonModule } from '@angular/common';
import { NgModule, ApplicationRef } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, FormControlDirective } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { removeNgStyles, createNewHosts, createInputTransfer } from '@angularclass/hmr';
import { JwtHelper } from 'angular2-jwt';

import { ENV_PROVIDERS } from './environment';
import { ROUTES } from './app.routes';
import { AppComponent } from './app.component';
import { AppState, InternalStateType } from './app.service';
import { NoContentComponent } from './no-content';
import { SettingsModule } from './settings';
import { PmodesModule } from './pmodes/pmodes.module';

import { As4ComponentsModule } from './common';
import { AuthenticationModule } from './authentication';
import { Http, RequestOptions } from '@angular/http';
import { AuthHttp } from 'angular2-jwt';
import { authHttpServiceFactory } from '../app/common/as4components.module';

import '../styles/external.scss';

type StoreType = {
    state: InternalStateType,
    restoreInputValues: () => void,
    disposeOldHosts: () => void
};

/**
 * `AppModule` is the main entry point into Angular2's bootstraping process
 */
@NgModule({
    bootstrap: [AppComponent],
    declarations: [
        AppComponent,
        NoContentComponent,
    ],
    imports: [ // import Angular's modules
        BrowserModule,
        FormsModule,
        HttpModule,
        CommonModule,
        RouterModule.forRoot(ROUTES, { useHash: false }),

        SettingsModule,
        PmodesModule,
        As4ComponentsModule,
        AuthenticationModule,
        ClipboardModule
    ],
    providers: [ // expose our Services and Providers into Angular's dependency injection
        AppState
    ]
})
export class AppModule {
    constructor(public appRef: ApplicationRef, public appState: AppState) { }

    public hmrOnInit(store: StoreType) {
        if (!store || !store.state) {
            return;
        }
        console.log('HMR store', JSON.stringify(store, null, 2));
        // set state
        this.appState._state = store.state;
        // set input values
        if ('restoreInputValues' in store) {
            let restoreInputValues = store.restoreInputValues;
            setTimeout(restoreInputValues);
        }

        this.appRef.tick();
        delete store.state;
        delete store.restoreInputValues;
    }

    public hmrOnDestroy(store: StoreType) {
        const cmpLocation = this.appRef.components.map((cmp) => cmp.location.nativeElement);
        // save state
        const state = this.appState._state;
        store.state = state;
        // recreate root elements
        store.disposeOldHosts = createNewHosts(cmpLocation);
        // save input values
        store.restoreInputValues = createInputTransfer();
        // remove styles
        removeNgStyles();
    }

    public hmrAfterDestroy(store: StoreType) {
        // display new elements
        store.disposeOldHosts();
        delete store.disposeOldHosts;
    }
}
