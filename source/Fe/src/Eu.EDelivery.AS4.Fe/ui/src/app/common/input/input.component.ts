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
    @Input() public label: string;
    @Input() public isLabelBold: boolean = true;
    @Input() public labelSize: number = 3;
    @Input() public controlSize: number = 5;
    @Input() public showLabel: boolean = true;
    @Input() public tooltip: string;
    @Input() public runtimeTooltip: string;
    constructor(private _runtimeStore: RuntimeStore) {

    }
    public ngOnChanges(changes: SimpleChanges) {
        if (changes['runtimeTooltip'] && changes['runtimeTooltip'].currentValue) {
            this._runtimeStore
                .changes
                .filter((state) => !!state && !!state.runtimeMetaData)
                .map((state) => state.runtimeMetaData)
                .subscribe((result) => this.tooltip = this.resolve(`${this.runtimeTooltip}.description`, result));
        }
    }

    private resolve(path: string, obj: any) {
        return path.split('.').reduce((prev, curr) => {
            return prev ? prev[curr] : undefined;
        }, obj || self);
    }
}
