import { Component, ViewEncapsulation, OnInit } from '@angular/core';

import { AppState } from './app.service';

// <nav>
//       <span>
//         <a [routerLink]=" ['./'] ">
//           Index
//         </a>
//       </span>
//       |
//       <span>
//         <a [routerLink]=" ['./home'] ">
//           Home
//         </a>
//       </span>
//       |
//       <span>
//         <a [routerLink]=" ['./detail'] ">
//           Detail
//         </a> 
//       </span>
//       |
//       <span>
//         <a [routerLink]=" ['./about'] ">
//           About
//         </a>
//       </span>
//     </nav>

@Component({
    selector: 'as4-app',
    encapsulation: ViewEncapsulation.None,
    styleUrls: [
        './app.component.css'
    ],
    template: `
  <div class="wrapper">
    <as4-header></as4-header>
    <as4-sidebar></as4-sidebar>
    <section class="content-wrapper">
        <section class="content">
            <router-outlet></router-outlet>
        </section>
    </section>
  </div>
  `
})
export class AppComponent implements OnInit {
    angularclassLogo = 'assets/img/angularclass-avatar.png';
    name = 'Angular 2 Webpack Starter';
    url = 'https://twitter.com/AngularClass';

    constructor(
        public appState: AppState) {

    }

    ngOnInit() {

    }
}