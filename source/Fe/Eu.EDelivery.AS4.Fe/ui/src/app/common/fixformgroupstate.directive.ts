import { ActivatedRoute } from '@angular/router';
import { Directive, AfterViewChecked, ElementRef, Renderer, NgZone } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
    selector: '[formControlName]'
})
export class FixFormGroupStatedirective implements AfterViewChecked {
    constructor(private _elementRef: ElementRef, private _renderer: Renderer, private _activeRoute: ActivatedRoute, private _ngZone: NgZone, private _ngControl: NgControl) { }
    public ngAfterViewChecked() {
        const currentState = this._elementRef.nativeElement.getAttribute('disabled');
        // Following code is a fix to handle the disabled state of newly added FormGroups
        // It will set the DOM disabled attribute to the correct state.
        // This will be done outside of the Angular scope to avoid triggering the change detection.
        // See https://github.com/angular/angular/issues/15206 for details
        if (!!this._ngControl) {
            if (this._ngControl.disabled !== !!currentState) {
                this._ngZone.runOutsideAngular(() => {
                    this._renderer.setElementAttribute(this._elementRef.nativeElement, 'disabled', !this._ngControl.disabled ? null! : this._ngControl.disabled + '');
                });
            }
        }
    }
}