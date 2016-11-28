import { AuthenticationStore } from './authentication/authentication.service';
import { Component, ViewEncapsulation, OnInit } from '@angular/core';

import { AppState } from './app.service';
import { RuntimeService } from './settings/runtime.service';

@Component({
    selector: 'as4-app',
    encapsulation: ViewEncapsulation.None,
    styles: [require('./app.component.scss').toString()],
    template: `
        <router-outlet></router-outlet>
  `
})
export class AppComponent implements OnInit {
    public isLoggedIn: boolean;
    constructor(public appState: AppState, private authenticationStore: AuthenticationStore, private runtimeService: RuntimeService) {
        this.authenticationStore.changes.subscribe(result => this.isLoggedIn = result.loggedin);
    }

    ngOnInit() {
        this.runtimeService.getReceivers();
        this.runtimeService.getSteps();
        this.runtimeService.getTransformers();
    }
}