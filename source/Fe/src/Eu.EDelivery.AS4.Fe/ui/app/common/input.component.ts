import { Component, Input } from '@angular/core';

@Component({
    selector: 'as4-input',
    template: `
        <div class="form-group">
            <label class="control-label col-sm-3">{{label}}</label>
            <div class="col-sm-9">
                <ng-content></ng-content>
            </div>
        </div>
    `
})
export class InputComponent {
    @Input() label: string;
    constructor() {
    }
}
