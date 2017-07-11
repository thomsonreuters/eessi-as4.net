import { Subscription } from 'rxjs/Subscription';
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

import { InputComponent } from './../input/input.component';

@Directive({
    selector: '[tabitem]'
})
export class TabItemDirective implements AfterViewInit {
    @Input() public title: string;
    @Input() public tabId: number;
    @Input() public isValid: boolean = true;
    constructor(private _elementRef: ElementRef, private _renderer: Renderer) {
        _renderer.setElementClass(this._elementRef.nativeElement, 'tab-pane', true);
    }
    public ngAfterViewInit() {
        this._renderer.setElementAttribute(this._elementRef.nativeElement, 'id', `tab_${this.tabId}`);
        if (this.tabId === 0) {
            this._renderer.setElementClass(this._elementRef.nativeElement, 'active', true);
        }
    }
    public setActive() {
        this._renderer.setElementClass(this._elementRef.nativeElement, 'active', true);
    }
    public setInactive(){
        this._renderer.setElementClass(this._elementRef.nativeElement, 'active', false);
    }
}
