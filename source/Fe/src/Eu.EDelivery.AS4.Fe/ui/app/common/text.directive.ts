import { Directive, ElementRef, Renderer } from '@angular/core';

@Directive({
    selector: 'input[type=text],input[type=number]'
})
export class TextDirective {
    constructor(private _elementRef: ElementRef, private _renderer: Renderer) {
        _renderer.setElementClass(_elementRef.nativeElement, 'form-control', true);
    }
}
