import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { ElementRef, OnInit, Directive, Renderer, ViewChild } from '@angular/core';

import * as $ from 'jquery';
// import 'select2';
import '../../../../node_modules/select2/dist/js/select2.js';

@Directive({
    selector: '[multiselect]',
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: MultiSelectDirective,
            multi: true
        }
    ]
})
export class MultiSelectDirective implements OnInit, ControlValueAccessor {
    private _propagateChange: (_: any) => {};
    private _select: any;
    private _el: any;
    private _currentVal: any;
    private _options: Array<{ option: ElementRef, value: string }>;
    constructor(private _element: ElementRef, private _renderer: Renderer) {
    }
    public ngOnInit() {
        this._el = <any>$(this._element.nativeElement);
        this._select = this._el.select2({
            theme: 'bootstrap',
            width: '100%',
            multiple: true
        })
            .on('change', (e) => {
            let val = this._el.val().map((data) => {
                return this.normalizeValue(data);
            });
            console.log(val);
            this._propagateChange(val);
            this._currentVal = val;
        });
        this.processOptions();
    }
    public writeValue(value: string[] | string) {
        if (!!!value) {
            this._el.val([]).trigger('change.select2');
            return;
        }

        if (!Array.isArray(value)) {
            value = value.split(',');
        }

        value.forEach((currentValue) => {
            let opt = this._options.find((option) => option.value === currentValue);
            if (!!opt) {
                this._renderer.setElementProperty(opt.option, 'selected', 'true');
            }
        });
        this._select.trigger('change.select2');
        this._currentVal = value;
    }
    public registerOnChange(fn) {
        this._propagateChange = fn;
    }
    public registerOnTouched() { }
    public setDisabledState(isDisabled: boolean) { }
    private normalizeValue(value: string): string {
        if (value.indexOf(':') === -1) {
            return value;
        }
        return value.split(': ')[1].replace(/'/g, '');
    }
    private processOptions() {
        if (!!!this._options) {
            this._options = new Array<any>();
        } else {
            return;
        }
        for (let option of this._element.nativeElement.options) {
            this._options.push({
                option,
                value: option.value
            });
        }
    }
}
