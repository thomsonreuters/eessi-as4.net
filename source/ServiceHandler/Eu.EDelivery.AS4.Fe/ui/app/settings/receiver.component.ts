import { Component, Input } from '@angular/core';

import { Receiver } from './../api/Receiver';

@Component({
    selector: 'as4-receiver',
    template: `
        <p>Type: {{settings?.type}}</p>
        <h3>Receiver</h3>
        <div *ngFor="let setting of settings?.setting">
            <p>Value: {{setting?.value}}</p>
            <p>Key: {{setting?.key}}</p>
        </div>
    `
})
export class ReceiverComponent {
    @Input() settings: Receiver;
}
