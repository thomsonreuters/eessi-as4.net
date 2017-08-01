import { Directive, Input, SkipSelf, Optional } from '@angular/core';

@Directive({ selector: '[runtimetooltip]' })
export class RuntimetoolTipDirective {
    @Input() public runtimetooltip: string;
    constructor( @SkipSelf() @Optional() private _parentTooltip: RuntimetoolTipDirective) {
    }
    public getPath(): string {
        if (!!this._parentTooltip) {
            return `${this._parentTooltip.getPath()}.${this.runtimetooltip}`;
        }
        return this.runtimetooltip;
    }
}
