import { Component, Input, ElementRef, Renderer, AfterViewInit } from '@angular/core';

@Component({
    selector: 'as4-columns',
    template: `<ng-content class="items"></ng-content>`,
    styles: [`
        @media screen and (min-width: 960px) {
            :host {
                display: flex;
            }
            :host.has-margin >>> *:not(:last-child) {
                margin-right: 11px;
            }
            :host >>> * {
                flex: 1;
            }
            :host:not(.has-margin) >>> * {
                display: inline-table
            }
            :host >>> *:last-child {
                flex: 0 auto;
            }
        }
        @media screen and (max-width: 959px) {
            :host >>> *:not(.has-margin) {
                margin-top: 9px;
            }
        }
    `]
})
export class ColumnsComponent {
    @Input() noMargin: boolean = false;
    constructor(private _element: ElementRef, private _renderer: Renderer) { }
    ngOnInit() {
        if (!this.noMargin) this._renderer.setElementClass(this._element.nativeElement, 'has-margin', true);
    }
}
