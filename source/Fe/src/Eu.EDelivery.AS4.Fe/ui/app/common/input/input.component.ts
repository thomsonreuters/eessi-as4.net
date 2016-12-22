import { Component, Input } from '@angular/core';

@Component({
    selector: 'as4-input',
    template: `
        <div class="form-group" [class.isBoldLabel]="isLabelBold">
            <label class="control-label col-xs-12 col-md-{{labelSize}}" *ngIf="showLabel">{{label}}<ng-content select="[label]"></ng-content></label>
            <div class="col-xs-12 col-md-{{controlSize}}">
                <ng-content></ng-content>
            </div>
        </div>
    `,
    styles: [`
        .isBoldLabel > label {
            font-weight: bold;
        }
    `]
})
export class InputComponent {
    @Input() label: string;
    @Input() isLabelBold: boolean = true;
    @Input() labelSize: number = 3;
    @Input() controlSize: number = 5;
    @Input() showLabel: boolean = true;
}
