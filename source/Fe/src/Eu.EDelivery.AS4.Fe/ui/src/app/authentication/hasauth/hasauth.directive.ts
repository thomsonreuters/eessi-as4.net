import { FormGroupDirective, ControlContainer, AbstractControl } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs/Subscription';
import { Directive, ElementRef, Renderer, OnDestroy, Input, Optional, OnInit } from '@angular/core';

import { AuthenticationStore } from './../authentication.store';

interface IAuthCheck {
    isAuthCheck: boolean;
}

@Directive({
    selector: 'input:not([as4-no-auth]), select:not([as4-no-auth])'
})
export class HasAuthDirective implements OnDestroy, OnInit {
    @Input() public required: string = 'readonly';
    private _subscription: Subscription;
    constructor(private _elementRef: ElementRef, private _authenticationStore: AuthenticationStore, private _renderer: Renderer, private _activeRoute: ActivatedRoute, @Optional() private _formGroup: ControlContainer) {
        if ((<IAuthCheck>this._activeRoute.snapshot.data).isAuthCheck === false) {
            return;
        }

        this._subscription = this._authenticationStore
            .changes
            .map((store) => store.roles)
            .map((role) => !!role.find((check) => check === this.required))
            .subscribe((isReadOnly) => {
                this._renderer.setElementAttribute(this._elementRef.nativeElement, 'readonly', isReadOnly ? 'true' : '');
                this._renderer.setElementAttribute(this._elementRef.nativeElement, 'readonly', isReadOnly ? 'true' : '');
            });
    }
    public ngOnDestroy() {
        this._subscription.unsubscribe();
    }
    public ngOnInit() {
        if (!!this._formGroup) {
            let root = <AbstractControl>(!!!this._formGroup.control ? this._formGroup : this._formGroup.control.root);

            root.statusChanges.subscribe(() => {
                root.disable();
            });
        }
    }
}
