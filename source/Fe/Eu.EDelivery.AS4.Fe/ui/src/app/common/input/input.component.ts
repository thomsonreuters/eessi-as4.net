import { Component, Input, OnInit, SimpleChanges, ChangeDetectionStrategy, Optional } from '@angular/core';
import { FormGroupName } from '@angular/forms';

import { RuntimeStore } from './../../settings/runtime.store';
import { RuntimetoolTipDirective } from './../runtimetooltip.directive';

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
export class InputComponent implements OnInit {
    @Input() public label: string;
    @Input() public isLabelBold: boolean = true;
    @Input() public labelSize: number = 3;
    @Input() public controlSize: number = 5;
    @Input() public showLabel: boolean = true;
    @Input() public tooltip: string;
    @Input() public formGroupName: string;
    constructor(private _runtimeStore: RuntimeStore, @Optional() private _tooltipDirective: RuntimetoolTipDirective) { }
    public ngOnInit() {
        if (!!!this._tooltipDirective) {
            return;
        }
        this._runtimeStore
            .changes
            .filter((state) => !!state && !!state.runtimeMetaData)
            .map((state) => state.runtimeMetaData)
            .take(1)
            .subscribe((result) => {
                const runtime = result[this._tooltipDirective.getPath()];
                if (!!!runtime) {
                    return;
                }
                this.tooltip = result[this._tooltipDirective.getPath()].description;
            });
    }
}
