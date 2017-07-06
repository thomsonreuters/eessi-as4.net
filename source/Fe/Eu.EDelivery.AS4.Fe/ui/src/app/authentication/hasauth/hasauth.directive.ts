import {
    FormGroupDirective,
    ControlContainer,
    AbstractControl,
    ControlValueAccessor,
    NG_VALUE_ACCESSOR,
    FormControlName,
    FormGroup,
    NgForm,
    NgControl
} from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs/Subscription';
import {
    Directive,
    ElementRef,
    Renderer,
    OnDestroy,
    Input,
    Optional,
    OnInit,
    HostListener,
    Inject,
    forwardRef,
    OnChanges,
    Self,
    AfterViewChecked,
    ViewChildren,
    QueryList
} from '@angular/core';

import { AuthenticationStore } from './../authentication.store';

interface IAuthCheck {
    isAuthCheck: boolean;
}

@Directive({
    selector: 'input:not([as4-no-auth]), select:not([as4-no-auth]), [as4-auth][disabled], [as4-auth]',
    exportAs: 'auth'
})
export class HasAuthDirective implements OnDestroy, AfterViewChecked {
    @Input() public required: string = 'admin';
    @Input() public disabled: boolean;
    @Input() public formControlName: string;
    private _subscription: Subscription;
    private _isReadOnly: boolean = false;
    private _enabled: boolean;
    constructor(private _elementRef: ElementRef, private _authenticationStore: AuthenticationStore, private _renderer: Renderer, private _activeRoute: ActivatedRoute,
        @Optional() @Self() @Inject(NG_VALUE_ACCESSOR) private valueAccessors: ControlValueAccessor[], @Self() @Optional() private ngControl: NgControl) {
        this._enabled = (<IAuthCheck>this._activeRoute.snapshot.data).isAuthCheck === undefined;
        if (this._enabled === false) {
            return;
        }

        this._subscription = this._authenticationStore
            .changes
            .map((store) => store.roles)
            .filter((role) => !!!role.find((check) => check === this.required))
            .subscribe(() => {
                this._isReadOnly = true;
                this._renderer.setElementAttribute(this._elementRef.nativeElement, 'readonly', 'true');
                this._renderer.setElementAttribute(this._elementRef.nativeElement, 'disabled', 'true');
            });
    }
    public ngOnDestroy() {
        if (!!!this._subscription) {
            return;
        }
        this._subscription.unsubscribe();
    }
    public ngAfterViewChecked() {
        if (!!this.ngControl) {
            this._renderer.setElementAttribute(this._elementRef.nativeElement, 'disabled', this.ngControl.disabled + '');
        }
        if (!!!this.valueAccessors) {
            this._renderer.setElementAttribute(this._elementRef.nativeElement, 'disabled', this.disabled ? 'true' : null);
            return;
        }

        if (this._enabled && this._isReadOnly) {
            this._renderer.setElementAttribute(this._elementRef.nativeElement, 'disabled', 'true');
            this._renderer.setElementAttribute(this._elementRef.nativeElement, 'readonly', 'true');
            if (!!this.valueAccessors) {
                this.valueAccessors.forEach((ac) => ac.setDisabledState(true));
            }
        } else {
            // this._renderer.setElementAttribute(this._elementRef.nativeElement, 'disabled', this.disabled ? 'true' : null);
            // this._renderer.setElementAttribute(this._elementRef.nativeElement, 'readonly', this.disabled ? 'true' : null);
            if (!!this.valueAccessors) {
                this.valueAccessors.forEach((ac) => ac.setDisabledState(!!!this.disabled ? false : this.disabled));
            }
        }
    }
}
