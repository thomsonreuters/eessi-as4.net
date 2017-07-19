import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subscription } from 'rxjs/Subscription';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { ElementRef, OnInit, Directive, Renderer, ViewChild, Input, OnDestroy, ViewChildren, QueryList, ContentChildren, AfterContentInit, ChangeDetectorRef } from '@angular/core';

import * as $ from 'jquery';
import '../../../../node_modules/select2/dist/js/select2.js';

@Directive({
    selector: 'option'
})
export class OptionDirective {
    constructor() { }
}

// tslint:disable-next-line:max-classes-per-file
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
export class MultiSelectDirective implements OnInit, ControlValueAccessor, OnDestroy, AfterContentInit {
    @Input() public multiple: boolean = true;
    @Input() public data: Observable<any>;

    @ContentChildren(OptionDirective) public children: QueryList<OptionDirective>;

    private _propagateChange: (_: any) => {};
    private _select: any;
    private _el: any;
    private _options: Array<{ option: ElementRef, value: string }>;

    private _subscription: Subscription;
    private _isInited: BehaviorSubject<boolean> = new BehaviorSubject(false);
    private _currentValue: BehaviorSubject<string[] | null> = new BehaviorSubject(null);
    private _hasData: BehaviorSubject<boolean> = new BehaviorSubject(false);
    constructor(private _element: ElementRef, private _renderer: Renderer, private _changeDetectorRef: ChangeDetectorRef) { }
    public ngAfterContentInit() {
        this.children.changes.subscribe(() => this._hasData.next(this.children.length > 0));
        this._hasData.next(this.children.length > 0);
    }
    public ngOnInit() {
        this._subscription = Observable
            .combineLatest(this._isInited, this._currentValue, this._hasData)
            .filter((result) => result[0] && result[2])
            .subscribe((result) => {
                let value = result[1];
                if (!!!value) {
                    this._select.val(null).trigger('change');
                    return;
                }
                this._select.val([...value]).trigger('change');
            });
        this._el = <any>$(this._element.nativeElement);
        this._select = this._el.select2({
            width: '100%',
            multiple: this.multiple
        })
            .on('select2:select select2:unselect', (e) => {
                let val = this._el.val();
                if (Array.isArray(val)) {
                    if (val.length === 0) val = null;
                    else val = val.map((data) => this.normalizeValue(data));
                } else {
                    val = this.normalizeValue(val);
                }

                this._propagateChange(!!!val ? null : [...val]);
                this._changeDetectorRef.detectChanges();
            });
        this._isInited.next(true);
    }
    public ngOnDestroy() {
        if (!!this._subscription) {
            this._subscription.unsubscribe();
        }
        this._el.select2('destroy');
    }
    public writeValue(value: string[] | null) {
        this._currentValue.next(value);
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
}
