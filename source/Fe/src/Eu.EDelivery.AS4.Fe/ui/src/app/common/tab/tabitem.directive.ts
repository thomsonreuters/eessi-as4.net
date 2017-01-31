import { InputComponent } from './../input/input.component';
import {
    Directive,
    Input,
    ElementRef,
    Renderer,
    ChangeDetectionStrategy,
    AfterViewInit,
    ContentChild,
    TemplateRef
} from '@angular/core';

@Directive({
    selector: '[tabitem]'
})
export class TabItemDirective implements AfterViewInit {
    @Input() public title: string;
    @Input() public tabId: number;
    @Input() public isValid: boolean = true;
    @ContentChild('headerExtra') public container: TemplateRef<Object>;
    constructor(private _elementRef: ElementRef, private _renderer: Renderer) {
        _renderer.setElementClass(this._elementRef.nativeElement, 'tab-pane', true);
    }
    public ngAfterViewInit() {
        this._renderer.setElementAttribute(this._elementRef.nativeElement, 'id', `tab_${this.tabId}`);
        if (this.tabId === 0) {
            this._renderer.setElementClass(this._elementRef.nativeElement, 'active', true);
        }
    }
}
