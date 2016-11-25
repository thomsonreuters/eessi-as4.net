import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'as4-wrapper',
    template: `<div class="wrapper">
        <as4-header></as4-header>
        <as4-sidebar></as4-sidebar>
        <section class="content-wrapper">
            <section class="content-header">
                {{breadCrumb}}
            </section>
            <section class="content">
                <router-outlet></router-outlet>
            </section>
        </section>
    </div>`,
    styles: [
        `
            .content-header {
                font-size: 12px !important;
            }
        `
    ]
})
export class WrapperComponent implements OnInit {
    public breadCrumb: string;
    constructor(private activatedRoute: ActivatedRoute) {
        activatedRoute.url.subscribe(result => {
            this.breadCrumb = this.getPath(this.activatedRoute);
        });
    }
    private getPath(activatedRoute: ActivatedRoute): string {
        let path = activatedRoute && activatedRoute.data && (<any>activatedRoute.data).value['title'];
        if (!!activatedRoute.firstChild) {
            path += this.getPath(activatedRoute.children[0]);
        }

        console.log(activatedRoute.parent);
        if (!!!(<any>activatedRoute.parent.data).value['title']) {
            return path;
        }
        return ` > ${path}`;
    }
    ngOnInit() {
    }
}