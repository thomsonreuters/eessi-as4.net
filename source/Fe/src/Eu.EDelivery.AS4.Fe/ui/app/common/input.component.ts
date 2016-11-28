import { Component, OnInit, Input } from '@angular/core';

@Component({
    selector: 'as4-input',
    template: `
        <div class="form-group">
            <label class="control-label col-sm-2">{{label}}</label>
            <div class="col-sm-10">
                <ng-content></ng-content>
            </div>
        </div>
    `
})
export class InputComponent implements OnInit {
    @Input() label: string;
    constructor() {
    }

    ngOnInit() {
    }
}