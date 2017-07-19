import {
    FormGroupDirective,
    ControlContainer,
    AbstractControl,
    FormControlName,
    FormGroup,
    NgForm,
    NgControl,
    NG_VALUE_ACCESSOR
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
    ViewChildren,
    QueryList,
    NgZone,
    DoCheck,
    ApplicationRef
} from '@angular/core';

import { AuthenticationStore } from './../authentication.store';

interface IAuthCheck {
    isAuthCheck: boolean;
}

@Directive({
    selector: 'input:not([as4-no-auth]), select:not([as4-no-auth]), [as4-auth][disabled], [as4-auth]',
    exportAs: 'auth'
})
export class HasAuthDirective implements OnDestroy, DoCheck {
    @Input() public required: string = 'admin';
    @Input() public disabled: boolean;
    @Input() public formControlName: string;
    private _subscription: Subscription | null;
    private _isReadOnly: boolean = false;
    private _enabled: boolean;
    private _previous: boolean | null = null;
    constructor(private _elementRef: ElementRef, private _authenticationStore: AuthenticationStore, private _renderer: Renderer, private _activeRoute: ActivatedRoute,
        @Self() @Optional() private _ngControl: NgControl, private _ngZone: NgZone, private _appRef: ApplicationRef) {
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
        if (!!this._subscription) {
            this._subscription.unsubscribe();
        }
    }
    public hasAccess(): boolean {
        return !(this._enabled && this._isReadOnly);
    }
    public ngDoCheck() {
        if (!this._enabled) {
            return;
        }
        const currentState = this._elementRef.nativeElement.disabled;
        if (this._isReadOnly && this._isReadOnly !== (!!!currentState ? false : true)) {
            // State doesn't match update it
            this._ngZone.runOutsideAngular(() => {
                this._renderer.setElementAttribute(this._elementRef.nativeElement, 'disabled', 'true');
            });
        }
    }
}
