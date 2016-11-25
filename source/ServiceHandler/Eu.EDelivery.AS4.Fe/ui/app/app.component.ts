import { AuthenticationStore } from './authentication/authentication.service';
import { Component, ViewEncapsulation, OnInit } from '@angular/core';

import { AppState } from './app.service';

// <div class="wrapper">
//         <as4-header></as4-header>
//         <as4-sidebar></as4-sidebar>
//         <section class="content-wrapper">
//             <section class="content">
//                 <router-outlet></router-outlet>
//             </section>
//         </section>
//     </div>

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
    constructor(public appState: AppState, private authenticationStore: AuthenticationStore) {
        this.authenticationStore.changes.subscribe(result => this.isLoggedIn = result.loggedin);
    }

    ngOnInit() {

    }
}