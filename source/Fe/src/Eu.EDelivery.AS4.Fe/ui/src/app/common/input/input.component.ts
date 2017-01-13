import { Component, Input, AfterViewInit, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';

import { RuntimeStore } from './../../settings/runtime.store';

@Component({
    selector: 'as4-input',
    template: `
        <div class="form-group" [class.isBoldLabel]="isLabelBold">
            <label class="control-label col-xs-12 col-md-{{labelSize}}" *ngIf="showLabel">{{label}}<ng-content select="[label]"></ng-content><as4-info class="tooltip-info" [tooltip]="tooltip"></as4-info></label>
            <div class="col-xs-12 col-md-{{controlSize}}">
                <ng-content></ng-content>
            </div>
        </div>
    `,
    styles: [`
        .isBoldLabel > label {
            font-weight: bold;
        }
        .tooltip-info {
            margin-left: 5px;
        }
    `],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class InputComponent {
    @Input() label: string;
    @Input() isLabelBold: boolean = true;
    @Input() labelSize: number = 3;
    @Input() controlSize: number = 5;
    @Input() showLabel: boolean = true;
    @Input() tooltip: string;
    @Input() runtimeTooltip: string;
    constructor(private _runtimeStore: RuntimeStore) {

    }
    ngOnChanges(changes: SimpleChanges) {
        if (changes['runtimeTooltip'] && changes['runtimeTooltip'].currentValue) {
            this._runtimeStore
                .changes
                .filter(state => !!state && !!state.runtimeMetaData)
                .map(state => state.runtimeMetaData)
                .subscribe(result => {
                    this.tooltip = this.resolve(`${this.runtimeTooltip}.description`, result);
                });
        }
    }

    private resolve(path: string, obj: any) {
        return path.split('.').reduce(function (prev, curr) {
            return prev ? prev[curr] : undefined;
        }, obj || self);
    }
}
