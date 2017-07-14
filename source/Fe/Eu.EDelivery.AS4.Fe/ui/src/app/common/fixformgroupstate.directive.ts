import { ActivatedRoute } from '@angular/router';
import { Directive, OnChanges, ElementRef, Renderer, NgZone, ChangeDetectorRef, Optional, Self, Inject, DoCheck } from '@angular/core';
import { NgControl, AbstractControl, ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import { HasAuthDirective } from './../authentication/hasauth/hasauth.directive';

@Directive({
    selector: '[formControlName], [formControl]'
})
export class FixFormGroupStateDirective implements DoCheck {
    private _previous: boolean | null = null;
    // tslint:disable-next-line:max-line-length
    constructor(private _ngControl: NgControl, @Optional() @Self() @Inject(NG_VALUE_ACCESSOR) private valueAccessors: ControlValueAccessor[], @Optional() private _hasAuth: HasAuthDirective) { }
    public ngDoCheck() {
        if (!!this._hasAuth && !this._hasAuth.hasAccess()) {
            return;
        }
        if (this._previous !== this._ngControl.disabled) {
            this._previous = this._ngControl.disabled;
            this.valueAccessors.forEach((accessor) => accessor!.setDisabledState!(this._ngControl!.disabled!));
        }
    }
}
