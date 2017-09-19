import { Directive, Input, SkipSelf, Optional } from '@angular/core';

@Directive({ selector: '[runtimeTooltip]' })
export class RuntimetoolTipDirective {
    @Input() public runtimeTooltip: string;
    constructor( @SkipSelf() @Optional() private _parentTooltip: RuntimetoolTipDirective) {
    }
    public getPath(): string {
        let result: string | null = null;
        if (!!this._parentTooltip) {
            result = `${this._parentTooltip.getPath()}.${this.runtimeTooltip}`;
        } else {
            result = this.runtimeTooltip;
        }
        return result.toLocaleLowerCase();
    }
}
