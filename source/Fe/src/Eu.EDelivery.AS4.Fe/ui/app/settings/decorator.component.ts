import { Component, Input } from '@angular/core';

import { Decorator } from './../api/Decorator';

@Component({
    selector: 'as4-decorator',
    template: `
        <p>Type: {{settings?.type}}</p>
        <h3>Steps</h3>
        <div *ngFor="let step of settings?.steps">
            <p>Type: {{step.type}}</p>
            <p>Value: {{step.value}}</p>
        </div>
    `
})
export class DecoratorComponent {
    @Input() settings: Decorator;
}
