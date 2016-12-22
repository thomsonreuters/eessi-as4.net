import { Component } from '@angular/core';

@Component({
    selector: 'as4-columns',
    template: `
        <ng-content class="items"></ng-content>
    `,
    styles: [`
        @media screen and (min-width: 960px) {
            :host {
                display: flex;
            }
            :host >>> *:not(:last-child) {
                margin-right: 11px;
            }
            :host >>> * {
                flex: 1;
            }
        }
        @media screen and (max-width: 959px) {
            :host >>> * {
                margin-top: 9px;
            }
        }
    `]
})
export class ColumnsComponent {
}
