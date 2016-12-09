import { Directive, ElementRef, AfterViewInit } from '@angular/core';

@Directive({
    selector: '[focus]'
})
export class FocusDirective implements AfterViewInit {
    constructor(elementRef: ElementRef) {
        console.log('Set focus');
        console.log(elementRef);
        elementRef.nativeElement.focus();
    }
    ngAfterViewInit() {
        console.log('ngAfterViewInit');
    }
}
