import { InputComponent } from './../input/input.component';
import { Directive, Input, ElementRef, Renderer } from '@angular/core';

@Directive({
    selector: '[tabitem]'
})
export class TabItemComponent {
    @Input() title: string;
    @Input() tabId: number;
    @Input() isValid: boolean = true;
    constructor(private _elementRef: ElementRef, private _renderer: Renderer) {
        _renderer.setElementClass(this._elementRef.nativeElement, 'tab-pane', true);
    }

    ngAfterViewInit() {
        this._renderer.setElementAttribute(this._elementRef.nativeElement, 'id', `tab_${this.tabId}`);
        if (this.tabId === 0) {
            this._renderer.setElementClass(this._elementRef.nativeElement, 'active', true);
        }
    }
}
