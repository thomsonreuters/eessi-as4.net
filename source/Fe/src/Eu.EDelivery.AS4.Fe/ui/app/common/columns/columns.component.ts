import { Component, Input, ElementRef, Renderer, AfterViewInit } from '@angular/core';

@Component({
    selector: 'as4-columns',
    template: `<ng-content class="items"></ng-content>`,
    styles: [`
        @media screen and (min-width: 960px) {
            :host {
                display: flex;
            }
            :host.no-margin >>> *:not(:last-child) {
                margin-right: 11px;
            }
            :host >>> * {
                flex: 1;
            }
            :host:not(.no-margin) >>> * {
                display: inline-table
            }
            :host:not(.no-margin) >>> *:last-child {
                flex: 0 auto !important;
            }
        }
        @media screen and (max-width: 959px) {
            :host >>> *:not(.no-margin) {
                margin-top: 9px;
            }
        }
    `]
})
export class ColumnsComponent {
    @Input() noMargin: boolean = false;
    constructor(private _element: ElementRef, private _renderer: Renderer) { }
    ngOnInit() {
        if (!this.noMargin) this._renderer.setElementClass(this._element.nativeElement, 'no-margin', true);
    }
}
