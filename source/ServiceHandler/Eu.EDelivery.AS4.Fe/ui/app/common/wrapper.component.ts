import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'as4-wrapper',
    template: `<div class="wrapper">
        <as4-header></as4-header>
        <as4-sidebar></as4-sidebar>
        <section class="content-wrapper">
            <section class="content">
                <router-outlet></router-outlet>
            </section>
        </section>
    </div>`
})
export class WrapperComponent implements OnInit {
    constructor() {
    }

    ngOnInit() {
    }
}