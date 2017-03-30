import { Directive, Input, ElementRef, Renderer, OnInit } from '@angular/core';

@Directive({
    selector: '[as4-tooltip]'
})
export class TooltipDirective implements OnInit {
    @Input('as4-tooltip') public input: string;
    constructor(private renderer: Renderer, private elementRef: ElementRef) {
        renderer.setElementAttribute(elementRef.nativeElement, 'data-toggle', 'tooltip');
        renderer.setElementClass(elementRef.nativeElement, 'as4-tooltip', true);
    }
    public ngOnInit() {
        this.renderer.setElementAttribute(this.elementRef.nativeElement, 'title', this.input);
    }
}
