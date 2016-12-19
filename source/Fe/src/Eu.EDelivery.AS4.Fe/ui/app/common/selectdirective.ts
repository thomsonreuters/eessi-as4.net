import { Directive, ElementRef, Renderer } from '@angular/core';

@Directive({
    selector: 'select'
})
export class SelectDirective {
    constructor(private _elementRef: ElementRef, private _renderer: Renderer) {
        _renderer.setElementClass(_elementRef.nativeElement, 'form-control', true);
    }
}
