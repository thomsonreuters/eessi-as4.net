import { Component, Input } from '@angular/core';

@Component({
    selector: 'as4-input',
    template: `
        <div class="form-group">
            <label class="control-label col-xs-{{labelSize}}" *ngIf="showLabel">{{label}}</label>
            <div class="col-xs-{{controlSize}}">
                <ng-content></ng-content>
            </div>
        </div>
    `
})
export class InputComponent {
    @Input() label: string;
    @Input() labelSize: number = 3;
    @Input() controlSize: number = 9;
    @Input() showLabel: boolean = true;
    constructor() {
    }
}
