import { NgControl } from '@angular/forms';
import { Directive, HostListener, ElementRef, OnInit, Input } from '@angular/core';

@Directive({
    selector: '[markAsTouched]'
})
export class MarkAsTouchedDirective implements OnInit {
    @Input() public noFocus: boolean = false;
    // tslint:disable-next-line:no-input-rename
    @Input('markAsTouched') public otherControl: NgControl;
    constructor(private _ngControl: NgControl, private _elementRef: ElementRef) { }
    @HostListener('click')
    public onClick() {
        this.ngOnInit();
    }
    public ngOnInit() {
        if (!!this._ngControl && !!this._ngControl.control) {
            if (!!this.otherControl && !!this.otherControl.control) {
                this.otherControl.control.markAsTouched();
                this.otherControl.control.markAsDirty();
                this.otherControl.control.updateValueAndValidity();
            }
            if (!this.noFocus) {
                if (document.activeElement === this._elementRef.nativeElement) {
                    setTimeout(() => {
                        this._elementRef.nativeElement.blur();
                        this._elementRef.nativeElement.focus()
                    });
                }
            }
        }
    }
}
