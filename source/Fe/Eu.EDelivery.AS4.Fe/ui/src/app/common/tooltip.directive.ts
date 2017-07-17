import { Directive, Input, ElementRef, Renderer, OnInit, OnDestroy, HostListener } from '@angular/core';
import * as $ from 'jquery';

@Directive({
    selector: '[as4-tooltip]',
    exportAs: 'as4-tooltip'
})
export class TooltipDirective implements OnInit, OnDestroy {
    // tslint:disable-next-line:no-input-rename
    @Input('as4-tooltip') public input: string;
    // tslint:disable-next-line:no-input-rename
    @Input('as4-tooltip-manual') public manual: boolean = false;
    // tslint:disable-next-line:no-input-rename
    @Input('as4-tooltip-placement') public placement: string = 'bottom';
    private custom: string | null;
    constructor(private renderer: Renderer, private elementRef: ElementRef) { }
    @HostListener('click')
    public onClick() {
        this.hide();
    }
    public ngOnInit() {
        (<any>$(this.elementRef.nativeElement)).tooltip({
            placement: this.placement,
            title: () => !!!this.custom ? this.input : this.custom,
            trigger: this.manual ? 'manual' : 'hover'
        });
    }
    public show(message: string | null = null) {
        let element = <any>$(this.elementRef.nativeElement);
        if (!!message) {
            this.custom = message;
        }
        element.tooltip('show');
        setTimeout(() => element.tooltip('hide'), 2000);
        this.custom = null;
    }
    public hide() {
        (<any>$(this.elementRef.nativeElement)).tooltip('hide');
    }
    public ngOnDestroy() {
        (<any>$(this.elementRef.nativeElement)).tooltip('destroy');
    }
}
